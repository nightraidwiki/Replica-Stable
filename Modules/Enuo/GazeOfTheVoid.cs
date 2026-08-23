using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Interop;
using Replica.Engine.Memory;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Engine.Managers;
using Replica.Engine.Helper;

namespace Replica.Modules.Enuo;

/// <summary>
/// Gaze of the Void (EX8 Enuo) – tower/orb solver.
/// Ported from the Splatoon script by NightmareXIV & mirage.
///
/// Logic:
///   1. Find tank orbs (DataId 19910) and DPS orbs (DataId 19909) that have
///      an active tether (stored in Data.TetherPlayer).
///   2. Sort DPS orbs clockwise starting from the first tank orb as reference.
///      Fast tethers (407) = first orbs to soak.  Slow (406) = second wave.
///   3. Highlight the orb assigned to the player based on config priority.
///   4. When the player has the vuln debuff (2941) show a countdown warning.
///   5. Increment PickedOrbs counter when the player is hit by a burst action.
/// </summary>
public class GazeOfTheVoid : ISpecialAction
{
	public override string Name => "Gaze of the Void";

	public override uint Phase => 1u;

	// 50005 = gaze cone cast, 50006/50007 = orb burst hits that count as "picked"
	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		50005u,
		50006u,
		50007u
	};

	// --- constants matching the Splatoon script ---
	private const uint TankBallDataId = 19910; // Large void soak (tank)
	private const uint DpsBallDataId  = 19909; // Small void soak (DPS/Healer)
	private const uint VulnStatusId   = 2941;  // Vulnerability Up
	private const uint TetherFastId   = 407;   // Fast tether
	private const uint TetherSlowId   = 406;   // Slow tether

	// Event-driven tether map: EntityId of orb -> tether ID (406 or 407)
	private readonly Dictionary<uint, uint> _tetherMap = new();

	// Sorted EntityId lists (populated once all 8 orbs are tethered)
	private List<uint> _fastBalls    = new();
	private List<uint> _slowBalls    = new();
	private uint       _fastTankBall = 0;
	private uint       _slowTankBall = 0;

	private int _pickedOrbs = 0;

	// Persistent guide VFX (created once per target, not per frame)
	private readonly List<StaticVfx> _guideVfx = new();
	private uint _currentGuideTarget = 0;

	// -----------------------------------------------------------------------
	// Config
	// -----------------------------------------------------------------------

	public override bool HasConfig => true;

	public override void DrawConfig()
	{
		ImGui.SetNextItemWidth(200f);
		int priority = Plugin.Config.EX8GazeOfTheVoidPriority;
		if (ImGui.SliderInt("DPS/Healer Priority", ref priority, 1, 3))
		{
			Plugin.Config.EX8GazeOfTheVoidPriority = priority;
			Plugin.Config.Save();
		}
		ImGui.TextWrapped("Your position as DPS/Healer, clockwise starting from tank orbs (irrelevant for tanks).");
		ImGui.Separator();
		ImGui.Text($"Picked orbs: {_pickedOrbs}");
		ImGui.Text($"Tether map: {_tetherMap.Count} orbs tracked");
		ImGui.Text($"Fast balls: {_fastBalls.Count}, Slow balls: {_slowBalls.Count}");
	}

	// -----------------------------------------------------------------------
	// Reset
	// -----------------------------------------------------------------------

	public override void Reset()
	{
		base.Reset();
		_tetherMap.Clear();
		_fastBalls.Clear();
		_slowBalls.Clear();
		_fastTankBall = 0;
		_slowTankBall = 0;
		_pickedOrbs   = 0;
		ClearGuide();
	}

	/// <summary>Remove all active guide VFX.</summary>
	private void ClearGuide()
	{
		foreach (var v in _guideVfx)
			v?.Remove();
		_guideVfx.Clear();
		_currentGuideTarget = 0;
	}

	// -----------------------------------------------------------------------
	// Gaze cone – draw fan omen on cast
	// -----------------------------------------------------------------------

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 50005u)
			SimpleElement.Fan(info, 45);
	}

	// -----------------------------------------------------------------------
	// Orb appearance – static circles so the player always sees them
	// -----------------------------------------------------------------------

	public override void OnObjectCreatedEvent(IGameObject go)
	{
		if (go.BaseId == DpsBallDataId)       // small (DPS)
			SimpleElement.Circle(go, 1.5f, 15000f, 0f, null, null, new Vector4(0f, 1f, 0f, 0.4f));
		else if (go.BaseId == TankBallDataId) // large (tank)
			SimpleElement.Circle(go, 2.0f, 15000f, 0f, null, null, new Vector4(1f, 0.4f, 0f, 0.4f));
	}

	// -----------------------------------------------------------------------
	// Tether events – the reliable way to track which orb has which tether
	// -----------------------------------------------------------------------

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != TetherFastId && Id != TetherSlowId)
			return;

		var go = actorId.GameObject();
		if (go == null)
			return;

		if (go.BaseId != DpsBallDataId && go.BaseId != TankBallDataId)
			return;

		_tetherMap[actorId] = Id;
		TryBuildSortedLists();
	}

	public override void OnActorTetherCancelEvent(uint actorID)
	{
		_tetherMap.Remove(actorID);
	}

	private void TryBuildSortedLists()
	{
		if (_fastBalls.Count == 3)
			return;

		var tankFast = new List<IGameObject>();
		var tankSlow = new List<IGameObject>();
		var dpsFast  = new List<IGameObject>();
		var dpsSlow  = new List<IGameObject>();

		foreach (var (entityId, tetherId) in _tetherMap)
		{
			var go = entityId.GameObject();
			if (go == null) continue;

			if (go.BaseId == TankBallDataId)
			{
				if (tetherId == TetherFastId) tankFast.Add(go);
				else                          tankSlow.Add(go);
			}
			else if (go.BaseId == DpsBallDataId)
			{
				if (tetherId == TetherFastId) dpsFast.Add(go);
				else                          dpsSlow.Add(go);
			}
		}

		if (tankFast.Count + tankSlow.Count != 2) return;
		if (dpsFast.Count != 3 || dpsSlow.Count != 3) return;

		var refOrb = tankFast.Concat(tankSlow).First();
		var center = new Vector2(100f, 100f);

		_fastBalls    = SortClockwise(dpsFast, center, refOrb.Position).Select(x => x.EntityId).ToList();
		_slowBalls    = SortClockwise(dpsSlow, center, refOrb.Position).Select(x => x.EntityId).ToList();
		_fastTankBall = tankFast.Count > 0 ? tankFast[0].EntityId : 0;
		_slowTankBall = tankSlow.Count > 0 ? tankSlow[0].EntityId : 0;
	}

	// -----------------------------------------------------------------------
	// Update – mirrors Splatoon's OnUpdate exactly
	// -----------------------------------------------------------------------

	public override void Update()
	{
		var localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer == null)
		{
			Reset();
			return;
		}

		if (_tetherMap.Count == 0)
		{
			if (_fastBalls.Count > 0 || _pickedOrbs > 0)
				Reset();
			return;
		}

		if (_fastBalls.Count != 3 || _slowBalls.Count != 3)
			return;

		bool isDpsOrHealer = !IsRole(localPlayer, 1);
		int  priority      = Plugin.Config.EX8GazeOfTheVoidPriority;

		if (_pickedOrbs == 0)
		{
			uint targetEntityId;
			if (isDpsOrHealer)
				targetEntityId = priority is >= 1 and <= 3 ? _fastBalls[priority - 1] : 0;
			else
				targetEntityId = _fastTankBall;

			SetGuide(localPlayer, targetEntityId, new Vector4(0f, 1f, 0f, 0.9f));
		}
		else if (_pickedOrbs == 1)
		{
			uint targetEntityId;
			if (isDpsOrHealer)
				targetEntityId = priority is >= 1 and <= 3 ? _slowBalls[priority - 1] : 0;
			else
				targetEntityId = _slowTankBall;

			float vulnRemaining = localPlayer.StatusList
				.FirstOrDefault(s => s.StatusId == VulnStatusId)?.RemainingTime ?? 0f;

			if (vulnRemaining > 0f)
				SimpleElement.ShowText($"!!! Wait {vulnRemaining:F1}s !!!", RaptureAtkModule.TextGimmickHintStyle.Warning, 1);

			SetGuide(localPlayer, targetEntityId, new Vector4(0f, 1f, 0f, 0.9f));
		}
		else
		{
			Reset();
		}
	}

	/// <summary>
	/// Create guide VFX anchored on the target orb. Only recreated when the target changes.
	/// Uses the orb as source (valid game object anchor) so VFX are stable.
	/// </summary>
	private void SetGuide(IGameObject player, uint targetEntityId, Vector4 color)
	{
		// Skip if guide already points to the right target
		if (targetEntityId == _currentGuideTarget)
			return;

		ClearGuide();

		if (targetEntityId == 0) return;

		var target = targetEntityId.GameObject();
		if (target == null) return;

		_currentGuideTarget = targetEntityId;

		// Large bright circle on the target orb – anchored on orb (stable)
		var circle = SimpleElement.Circle(target, 2.5f, 30000f, 0f, null, null, color);
		if (circle != null) _guideVfx.Add(circle);

		// Rectangle du joueur vers l'orbe, s'arrête au centre de l'orbe
		var arrow = DrawManager.Draw(new DrawElement
		{
			drawAvfx     = "customRect",
			radiusX      = 1f,
			radiusZ      = 1f,         // ignoré quand endToTarget = true
			drawOnObject = true,
			target       = target,
			endToTarget  = true,
			destroyTime  = 30000f,
			refColor        = color,
			refTargetColor  = color
		}, player);
		if (arrow != null) _guideVfx.Add(arrow);
	}

	// -----------------------------------------------------------------------
	// Orb burst hit detection – mirrors Splatoon's OnActionEffectEvent
	// -----------------------------------------------------------------------

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 50006u && info.ActionId != 50007u)
			return;

		var localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer == null) return;

		uint localEntityId = localPlayer.EntityId;

		bool hitMe = info.TargetEffects != null
			&& info.TargetEffects.Any(t => t.TargetID == localEntityId);

		if (!hitMe && info.Target != null)
			hitMe = info.Target.EntityId == localEntityId;

		if (hitMe)
			_pickedOrbs++;
	}

	// -----------------------------------------------------------------------
	// Helpers
	// -----------------------------------------------------------------------

	private static bool IsRole(IGameObject go, int role)
	{
		if (go is IBattleChara bc && bc.ClassJob.IsValid)
			return bc.ClassJob.Value.Role == role;
		return false;
	}

	/// <summary>
	/// Sorts <paramref name="objects"/> clockwise starting from the angle of
	/// <paramref name="reference"/> relative to <paramref name="center"/>.
	/// Mirrors Splatoon's MathHelper.EnumerateObjectsClockwise.
	/// </summary>
	private static List<IGameObject> SortClockwise(IEnumerable<IGameObject> objects, Vector2 center, Vector3 reference)
	{
		float refAngle = MathF.Atan2(reference.Z - center.Y, reference.X - center.X);
		return objects.OrderBy(x =>
		{
			float angle = MathF.Atan2(x.Position.Z - center.Y, x.Position.X - center.X);
			float diff  = angle - refAngle;
			while (diff < 0f) diff += 2f * MathF.PI;
			return diff;
		}).ToList();
	}
}

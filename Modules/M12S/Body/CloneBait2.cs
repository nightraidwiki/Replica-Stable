using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Windows;

namespace Replica.Modules.M12S.Body;

public class CloneBait2 : ISpecialAction
{
	public enum Strat
	{
		EU,
		JP,
		Banana
	}

	public enum Dir8
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}

	public enum Tether
	{
		Nothing = 0,
		Fan = 367,
		Stack = 369,
		Defamation = 368,
		Boss = 374
	}

	public enum SpotStyle
	{
		Circle,
		Pillar
	}

	public enum RoleMode
	{
		Auto,
		Melee,
		Ranged
	}

	public class Config
	{
		public bool Active;

		public Strat Strat;

		public RoleMode Role;

		public int ColorIndex = 4;

		public bool ShowTether = true;

		public bool ShowText = true;

		public bool ShowGrab = true;

		public bool ShowNorth = true;

		public bool ShowNothingGuide = true;

		public SpotStyle SpotStyle;

		public bool Preview;

		public Dir8 PreviewDir;

		public int PreviewPhase = 1;

		public bool PreviewNetherFar;

		public RoleMode PreviewRole = RoleMode.Melee;
	}

	private sealed class Group
	{
		public Dir8[] Dirs = Array.Empty<Dir8>();

		public Tether[] Tethers = Array.Empty<Tether>();

		public Dictionary<Tether, Vector2>[] Phase = new Dictionary<Tether, Vector2>[6];

		public Dictionary<Tether, Vector2>[] PhaseRanged = new Dictionary<Tether, Vector2>[6];

		public Dictionary<Tether, Vector2> Phase3Far = new Dictionary<Tether, Vector2>();
	}

	private sealed class Preset
	{
		public Group A = new Group();

		public Group B = new Group();

		public bool DifferentNetherwrath;

		public Dir8 North;
	}

	private const uint PlayerCloneBaseId = 19210u;

	private const uint SnakingKickId = 46375u;

	private const float Alpha = 0.85f;

	private static bool _enableMigrated;

	private uint _phase;

	private bool _netherFar;

	private uint _myCloneId;

	private Dir8? _dir;

	private readonly Dictionary<uint, uint> _cloneTethers = new Dictionary<uint, uint>();

	private StaticVfx _spot;

	private StaticVfx _grab;

	private uint _grabId;

	private StaticVfx _north;

	private StaticVfx _northDot;

	private StaticVfx _nothing;

	private Vector3? _spotAt;

	private SpotStyle _spotStyle;

	private const string GuideOwner = "m12s_rep2_guide";

	private bool _guideLive;

	private Vector3? _lastGuideSpot;

	private Tether _lastGuideTether;

	private bool _lastShowText;

	private bool _lastShowPath;

	private static readonly string[] StratNames = new string[3] { "EU", "JP", "Codex Banana" };

	private static readonly string[] DirNames = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	private static readonly string[] SpotStyleNames = new string[2] { "Circle", "Pillar" };

	private static readonly string[] RoleNames = new string[3] { "Auto", "Melee (T/M)", "Ranged (H/R)" };

	private static readonly HashSet<uint> RangedJobs = new HashSet<uint>
	{
		6u, 24u, 28u, 33u, 40u, 5u, 23u, 31u, 38u, 7u,
		26u, 25u, 27u, 35u, 36u, 42u
	};

	private static readonly Dictionary<Strat, Preset> Presets = BuildPresets();

	private static Config C => ModuleConfig.Get<Config>();

	private static Vector4 SpotColor
	{
		get
		{
			Vector4 result = StratUI.SwatchColor(C.ColorIndex);
			result.W = 0.85f;
			return result;
		}
	}

	private static Vector4 GrabColor => new Vector4(0.2f, 0.95f, 0.35f, 0.85f);

	private static Vector4 NorthColor => new Vector4(1f, 0.82f, 0.1f, 0.85f);

	private static Vector4 NothingColor => new Vector4(0.96f, 0.2f, 0.2f, 0.85f);

	public override string Name => "Replication 2 (Clones + Bait)";

	public override string? ModuleEnableKey => "Lindblum/Replication 2 (Clones + Bait)";

	public override uint Phase => 2u;

	public override bool HasConfig => true;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46307u, 46383u, 46311u, 46315u, 46384u, 47329u, 48733u };

	private static void EnsureEnableMigrated()
	{
		if (!_enableMigrated)
		{
			_enableMigrated = true;
			ModuleConfig.MigrateLegacyActive("Lindblum/Replication 2 (Clones + Bait)", C.Active);
		}
	}

	private static string SpotText(Tether tether)
	{
		return tether switch
		{
			Tether.Defamation => "BAIT DEFAMATION", 
			Tether.Stack => "STACK", 
			Tether.Fan => "FAN", 
			Tether.Boss => "BAIT BOSS", 
			Tether.Nothing => "FREE", 
			_ => "GO HERE", 
		};
	}

	private static bool IsRanged()
	{
		IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer != null)
		{
			return RangedJobs.Contains(localPlayer.ClassJob.RowId);
		}
		return false;
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 46307:
			if (_phase == 0)
			{
				_phase = 1u;
			}
			break;
		case 46383:
			_netherFar = true;
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 46311u:
			if (_phase == 1)
			{
				_phase = 2u;
			}
			break;
		case 46315u:
			if (_phase == 2)
			{
				_phase = 3u;
			}
			break;
		case 46384u:
			if (_phase == 3)
			{
				_phase = 4u;
			}
			break;
		case 47329u:
			if (_phase == 4)
			{
				_phase = 5u;
			}
			break;
		case 48733u:
			if (_phase >= 5)
			{
				_phase++;
			}
			break;
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint id, ulong targetId)
	{
		if ((id - 367 <= 2 || id == 374) ? true : false)
		{
			_cloneTethers[actorId] = id;
		}
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer != null && (uint)targetId == localPlayer.EntityId)
		{
			IGameObject gameObject = actorId.GameObject();
			if (gameObject != null && gameObject.BaseId == 19210)
			{
				_myCloneId = actorId;
			}
		}
	}

	public override void OnActorTetherCancelEvent(uint actorId)
	{
		_cloneTethers.Remove(actorId);
		if (actorId == _myCloneId)
		{
			_myCloneId = 0u;
		}
	}

	public override void Update()
	{
		if (!_dir.HasValue && _myCloneId != 0)
		{
			IGameObject gameObject = _myCloneId.GameObject();
			if (gameObject != null)
			{
				_dir = DirFromPos(gameObject.Position);
			}
		}
		Dir8? dir = (C.Preview ? new Dir8?(C.PreviewDir) : _dir);
		EnsureEnableMigrated();
		if (!C.Preview && !ModuleConfig.IsEnabled(ModuleEnableKey))
		{
			Clear();
			return;
		}
		if (!dir.HasValue)
		{
			Clear();
			return;
		}
		if (!C.Preview && _phase == 0)
		{
			RemoveSpot();
			ClearGuide();
			UpdateNorth(Presets[C.Strat].North);
			UpdateGrab(dir.Value);
			return;
		}
		UpdateGrab(Dir8.N, hide: true);
		if (C.Preview)
		{
			UpdateNorth(Presets[C.Strat].North);
		}
		else
		{
			RemoveNorth();
		}
		int num = (C.Preview ? Math.Clamp(C.PreviewPhase, 1, 5) : ((int)_phase));
		bool netherFar = (C.Preview ? C.PreviewNetherFar : _netherFar);
		if (num < 1 || num > 5)
		{
			Clear();
			return;
		}
		(int, Tether)? tuple = Resolve(dir.Value);
		if (!tuple.HasValue)
		{
			RemoveSpot();
			ClearGuide();
			return;
		}
		bool flag = (C.Preview ? (C.PreviewRole == RoleMode.Ranged) : (C.Role == RoleMode.Ranged || (C.Role == RoleMode.Auto && IsRanged())));
		Vector3? vector = SpotFor(num, dir.Value, netherFar, tuple.Value, flag);
		if (num == 3 && !C.Preview)
		{
			Vector3? vector2 = SnakingKickSafe();
			if (vector2.HasValue)
			{
				vector = vector2;
			}
		}
		if (num == 5)
		{
			Vector3? vector3 = Waymark((!flag) ? 1 : 3);
			if (vector3.HasValue)
			{
				vector = vector3;
			}
		}
		if (!vector.HasValue)
		{
			RemoveSpot();
			ClearGuide();
			return;
		}
		RefreshGuide(vector.Value, tuple.Value.Item2);
		if (_spot == null || !_spotAt.HasValue || _spotStyle != C.SpotStyle || Vector3.Distance(_spotAt.Value, vector.Value) > 0.05f)
		{
			RemoveSpot();
			_spot = SpawnSpot(vector.Value);
			_spotAt = vector.Value;
			_spotStyle = C.SpotStyle;
			if (_spot != null)
			{
				aoes.Add(_spot);
			}
		}
	}

	private (int lp, Tether tether)? Resolve(Dir8 dir)
	{
		Preset preset = Presets[C.Strat];
		for (int i = 0; i < preset.A.Dirs.Length; i++)
		{
			if (preset.A.Dirs[i] == dir)
			{
				return (0, preset.A.Tethers[i]);
			}
		}
		for (int j = 0; j < preset.B.Dirs.Length; j++)
		{
			if (preset.B.Dirs[j] == dir)
			{
				return (1, preset.B.Tethers[j]);
			}
		}
		return null;
	}

	private Vector3? SpotFor(int phase, Dir8 dir, bool netherFar, (int lp, Tether tether) r, bool ranged)
	{
		if (phase < 1 || phase > 5)
		{
			return null;
		}
		Preset preset = Presets[C.Strat];
		Group obj = ((r.lp == 0) ? preset.A : preset.B);
		Dictionary<Tether, Vector2> dictionary = ((((phase == 3) & netherFar) && preset.DifferentNetherwrath) ? obj.Phase3Far : ((!ranged || obj.PhaseRanged[phase] == null) ? obj.Phase[phase] : obj.PhaseRanged[phase]));
		if (dictionary != null && dictionary.TryGetValue(r.tether, out var value))
		{
			return new Vector3(value.X, 0f, value.Y);
		}
		return null;
	}

	private void UpdateNorth(Dir8 north)
	{
		if (!C.ShowNorth)
		{
			RemoveNorth();
		}
		else if (_north == null)
		{
			float x = (float)north * 45f * ((float)Math.PI / 180f);
			Vector3 vector = new Vector3(100f + 20f * MathF.Sin(x), 0f, 100f - 20f * MathF.Cos(x));
			_north = DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				radiusX = 0.5f,
				radiusY = 1f,
				radiusZ = 1f,
				drawOnObject = false,
				Position = new Vector3(100f, 0f, 100f),
				endToTarget = true,
				targetPosition = vector,
				refColor = NorthColor,
				refTargetColor = NorthColor,
				destroyTime = 600000f
			});
			if (_north != null)
			{
				aoes.Add(_north);
			}
			_northDot = DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customCircle",
				radiusX = 2.5f,
				radiusZ = 2.5f,
				drawOnObject = false,
				Position = vector,
				refColor = NorthColor,
				refTargetColor = NorthColor,
				destroyTime = 600000f
			});
			if (_northDot != null)
			{
				aoes.Add(_northDot);
			}
		}
	}

	private void UpdateGrab(Dir8 dir, bool hide = false)
	{
		if (hide || !C.ShowGrab)
		{
			RemoveGrab();
			RemoveNothing();
			return;
		}
		(int, Tether)? tuple = Resolve(dir);
		if (!tuple.HasValue)
		{
			RemoveGrab();
			RemoveNothing();
			return;
		}
		if (tuple.Value.Item2 == Tether.Nothing)
		{
			RemoveGrab();
			UpdateNothing();
			return;
		}
		RemoveNothing();
		uint num = FindGrabTarget(tuple.Value);
		if (num == 0)
		{
			RemoveGrab();
		}
		else
		{
			if (_grabId == num && _grab != null)
			{
				return;
			}
			RemoveGrab();
			IGameObject gameObject = num.GameObject();
			if (gameObject != null)
			{
				_grab = DrawManager.Draw(new DrawElement
				{
					drawAvfx = "customCircle",
					radiusX = 3.5f,
					radiusZ = 3.5f,
					radiusY = 3f,
					drawOnObject = true,
					refColor = GrabColor,
					refTargetColor = GrabColor,
					destroyTime = 600000f
				}, gameObject);
				_grabId = num;
				if (_grab != null)
				{
					aoes.Add(_grab);
				}
			}
		}
	}

	private uint FindGrabTarget((int lp, Tether tether) r)
	{
		uint item = (uint)r.tether;
		float num = (float)Presets[C.Strat].North * 45f - 5f;
		List<(uint, float)> list = new List<(uint, float)>();
		foreach (KeyValuePair<uint, uint> cloneTether in _cloneTethers)
		{
			if (cloneTether.Value == item)
			{
				IGameObject gameObject = cloneTether.Key.GameObject();
				if (gameObject != null)
				{
					float item2 = ((AngleCw(gameObject.Position) - num) % 360f + 360f) % 360f;
					list.Add((cloneTether.Key, item2));
				}
			}
		}
		if (list.Count == 0)
		{
			return 0u;
		}
		list.Sort(((uint id, float key) x, (uint id, float key) y) => x.key.CompareTo(y.key));
		if (r.lp != 0)
		{
			return list[list.Count - 1].Item1;
		}
		return list[0].Item1;
	}

	private void UpdateNothing()
	{
		if (!C.ShowNothingGuide || _myCloneId == 0)
		{
			RemoveNothing();
		}
		else
		{
			if (_nothing != null)
			{
				return;
			}
			IGameObject gameObject = _myCloneId.GameObject();
			if (gameObject != null)
			{
				_nothing = DrawManager.Draw(new DrawElement
				{
					drawAvfx = "customCircle",
					radiusX = 2f,
					radiusZ = 2f,
					drawOnObject = true,
					refColor = NothingColor,
					refTargetColor = NothingColor,
					destroyTime = 600000f
				}, gameObject);
				if (_nothing != null)
				{
					aoes.Add(_nothing);
				}
			}
		}
	}

	private static Vector3? SnakingKickSafe()
	{
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			try
			{
				if (!(item is IBattleChara battleChara) || !battleChara.IsValid() || !battleChara.IsCasting || battleChara.CastActionId != 46375)
				{
					continue;
				}
				float x = battleChara.Rotation + (float)Math.PI;
				return new Vector3(battleChara.Position.X + 3f * MathF.Sin(x), 0f, battleChara.Position.Z + 3f * MathF.Cos(x));
			}
			catch
			{
			}
		}
		return null;
	}

	private static float AngleCw(Vector3 p)
	{
		float num = MathF.Atan2(p.X - 100f, 0f - (p.Z - 100f)) * (180f / (float)Math.PI);
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	private static Dir8? DirFromPos(Vector3 p)
	{
		float num = p.X - 100f;
		float num2 = p.Z - 100f;
		if (MathF.Sqrt(num * num + num2 * num2) < 2f)
		{
			return null;
		}
		float num3 = AngleCw(p) / 45f;
		int value = (int)MathF.Round(num3) % 8;
		if (MathF.Abs(num3 - MathF.Round(num3)) > 0.25f)
		{
			return null;
		}
		return (Dir8)value;
	}

	private static StaticVfx SpawnSpot(Vector3 pos)
	{
		bool flag = C.SpotStyle == SpotStyle.Pillar;
		return DrawManager.Draw(new DrawElement
		{
			drawAvfx = (flag ? "co_trap00h1" : "customCircle"),
			radiusX = 1.3f,
			radiusZ = 1.3f,
			radiusY = 1f,
			drawOnObject = false,
			Position = pos,
			refColor = SpotColor,
			refTargetColor = SpotColor,
			destroyTime = 600000f
		});
	}

	private void RefreshGuide(Vector3 spot, Tether tether)
	{
		if (Plugin.Instance == null)
		{
			return;
		}
		bool flag = _lastGuideSpot.HasValue && Vector3.Distance(_lastGuideSpot.Value, spot) < 0.05f;
		if (!(_guideLive & flag) || _lastGuideTether != tether || _lastShowText != C.ShowText || _lastShowPath != C.ShowTether)
		{
			_guideLive = true;
			_lastGuideSpot = spot;
			_lastGuideTether = tether;
			_lastShowText = C.ShowText;
			_lastShowPath = C.ShowTether;
			LogEvent e = new LogEvent
			{
				Name = "rep2_guide"
			};
			Plugin.Instance.Engine.ClearExternal("m12s_rep2_guide");
			if (C.ShowText)
			{
				Plugin.Instance.Engine.SpawnExternal("m12s_rep2_guide", new DrawSpec
				{
					Shape = QuickShape.Text,
					Anchor = DrawAnchor.Self,
					AttachToActor = true,
					Color = SpotColor,
					Duration = 600f,
					Label = SpotText(tether),
					LabelColor = SpotColor,
					LabelSize = 1.2f,
					LabelHeight = 2f
				}, e, previewSelf: true);
			}
			if (C.ShowTether)
			{
				Plugin.Instance.Engine.SpawnExternal("m12s_rep2_guide", new DrawSpec
				{
					Shape = QuickShape.ChevronPath,
					Anchor = DrawAnchor.Self,
					AttachToActor = true,
					Link = LinkTarget.FixedSpot,
					LinkPosition = spot,
					Color = SpotColor,
					ChevronSpacing = 2f,
					LineThickness = 4f,
					Length = 30f,
					Duration = 600f
				}, e, previewSelf: true);
			}
		}
	}

	private void ClearGuide()
	{
		if (_guideLive)
		{
			_guideLive = false;
			_lastGuideSpot = null;
			Plugin.Instance?.Engine.ClearExternal("m12s_rep2_guide");
		}
	}

	private unsafe static Vector3? Waymark(int index)
	{
		MarkingController* ptr = MarkingController.Instance();
		if (ptr == null)
		{
			return null;
		}
		int num = 0;
		Span<FieldMarker> fieldMarkers = ptr->FieldMarkers;
		for (int i = 0; i < fieldMarkers.Length; i++)
		{
			ref FieldMarker reference = ref fieldMarkers[i];
			if (num == index)
			{
				if (!reference.Active)
				{
					return null;
				}
				return new Vector3((float)reference.X / 1000f, (float)reference.Y / 1000f, (float)reference.Z / 1000f);
			}
			num++;
		}
		return null;
	}

	private void RemoveSpot()
	{
		if (_spot != null)
		{
			_spot.Remove();
			aoes.Remove(_spot);
			_spot = null;
		}
		_spotAt = null;
	}

	private void RemoveGrab()
	{
		if (_grab != null)
		{
			_grab.Remove();
			aoes.Remove(_grab);
			_grab = null;
		}
		_grabId = 0u;
	}

	private void RemoveNorth()
	{
		if (_north != null)
		{
			_north.Remove();
			aoes.Remove(_north);
			_north = null;
		}
		if (_northDot != null)
		{
			_northDot.Remove();
			aoes.Remove(_northDot);
			_northDot = null;
		}
	}

	private void RemoveNothing()
	{
		if (_nothing != null)
		{
			_nothing.Remove();
			aoes.Remove(_nothing);
			_nothing = null;
		}
	}

	private void Clear()
	{
		RemoveSpot();
		ClearGuide();
		RemoveGrab();
		RemoveNorth();
		RemoveNothing();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
		_cloneTethers.Clear();
		_phase = 0u;
		_netherFar = false;
		_myCloneId = 0u;
		_dir = null;
	}

	public override void DrawConfig()
	{
		EnsureEnableMigrated();
		bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
		if (StratUI.Header("Replication 2 — Clones", ref active))
		{
			ModuleConfig.SetEnabled(ModuleEnableKey, active);
			C.Active = active;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Strategy");
		int selected = (int)C.Strat;
		if (StratUI.SegmentedBar(StratNames, ref selected))
		{
			C.Strat = (Strat)selected;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint($"Relative north for this strat: {Presets[C.Strat].North}. Side is read live from your clone tether.");
		StratUI.Section("Role group (final stacks)");
		int selected2 = (int)C.Role;
		if (StratUI.SegmentedBar(RoleNames, ref selected2))
		{
			C.Role = (RoleMode)selected2;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint((C.Role == RoleMode.Auto) ? ("Auto from your job — currently " + (IsRanged() ? "Ranged (H/R)" : "Melee (T/M)") + ". Tanks + melee stack one side, healers + ranged the other.") : "Tanks + melee stack one side; healers + ranged the other. Only changes the last two stacks.");
		StratUI.Section("Marker color");
		int index = C.ColorIndex;
		if (StratUI.ColorSwatches(ref index))
		{
			C.ColorIndex = index;
			ModuleConfig.Save<Config>();
		}
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled(StratUI.SwatchName(C.ColorIndex));
		StratUI.Section("Spot style");
		int selected3 = (int)C.SpotStyle;
		if (StratUI.SegmentedBar(SpotStyleNames, ref selected3))
		{
			C.SpotStyle = (SpotStyle)selected3;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint("Pillar is a tall beam of light — stays visible above other ground AoEs.");
		StratUI.Section("Show");
		bool v = C.ShowGrab;
		if (ImGui.Checkbox("Clone to grab the tether from (green)", ref v))
		{
			C.ShowGrab = v;
			ModuleConfig.Save<Config>();
		}
		bool v2 = C.ShowNorth;
		if (ImGui.Checkbox("Strat north line (yellow)", ref v2))
		{
			C.ShowNorth = v2;
			ModuleConfig.Save<Config>();
		}
		bool v3 = C.ShowTether;
		if (ImGui.Checkbox("Path to my spot", ref v3))
		{
			C.ShowTether = v3;
			ModuleConfig.Save<Config>();
		}
		bool v4 = C.ShowText;
		if (ImGui.Checkbox("Callout text over me (STACK / BAIT …)", ref v4))
		{
			C.ShowText = v4;
			ModuleConfig.Save<Config>();
		}
		bool v5 = C.ShowNothingGuide;
		if (ImGui.Checkbox("Mark my clone when I take no tether", ref v5))
		{
			C.ShowNothingGuide = v5;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Preview");
		bool v6 = C.Preview;
		if (ImGui.Checkbox("Preview in arena (ignores phase, for testing)", ref v6))
		{
			C.Preview = v6;
			ModuleConfig.Save<Config>();
		}
		if (C.Preview)
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Your clone direction:");
			int selected4 = (int)C.PreviewDir;
			if (StratUI.SegmentedBar(DirNames, ref selected4))
			{
				C.PreviewDir = (Dir8)selected4;
				ModuleConfig.Save<Config>();
			}
			int v7 = C.PreviewPhase;
			ImGui.SetNextItemWidth(220f);
			if (ImGui.SliderInt("Phase", ref v7, 1, 5))
			{
				C.PreviewPhase = Math.Clamp(v7, 1, 5);
				ModuleConfig.Save<Config>();
			}
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Role group:");
			int selected5 = ((C.PreviewRole == RoleMode.Ranged) ? 1 : 0);
			if (StratUI.SegmentedBar(new string[2] { "Melee (T/M)", "Ranged (H/R)" }, ref selected5))
			{
				C.PreviewRole = ((selected5 != 1) ? RoleMode.Melee : RoleMode.Ranged);
				ModuleConfig.Save<Config>();
			}
			if (Presets[C.Strat].DifferentNetherwrath)
			{
				bool v8 = C.PreviewNetherFar;
				if (ImGui.Checkbox("Netherwrath far (phase 3)", ref v8))
				{
					C.PreviewNetherFar = v8;
					ModuleConfig.Save<Config>();
				}
			}
			(int, Tether)? tuple = Resolve(C.PreviewDir);
			if (!tuple.HasValue)
			{
				ImGui.TextDisabled("This direction is unused by the selected strat.");
			}
			else
			{
				Vector3? vector = SpotFor(Math.Clamp(C.PreviewPhase, 1, 5), C.PreviewDir, C.PreviewNetherFar, tuple.Value, C.PreviewRole == RoleMode.Ranged);
				string value = ((tuple.Value.Item1 == 0) ? "Group 1" : "Group 2");
				string value2 = ((!vector.HasValue) ? "(none)" : $"({vector.Value.X:0.0}, {vector.Value.Z:0.0})");
				ImU8String text = new ImU8String(18, 3);
				text.AppendFormatted(value);
				text.AppendLiteral(" · tether ");
				text.AppendFormatted(tuple.Value.Item2);
				text.AppendLiteral(" · spot ");
				text.AppendFormatted(value2);
				ImGui.TextDisabled(text);
			}
		}
		if (!ImGui.CollapsingHeader("Debug"))
		{
			return;
		}
		ImU8String text2 = new ImU8String(25, 2);
		text2.AppendLiteral("Phase ");
		text2.AppendFormatted(_phase);
		text2.AppendLiteral("   Netherwrath far ");
		text2.AppendFormatted(_netherFar);
		ImGui.TextUnformatted(text2);
		ImU8String text3 = new ImU8String(25, 2);
		text3.AppendLiteral("My clone id ");
		text3.AppendFormatted(_myCloneId);
		text3.AppendLiteral("   Direction ");
		text3.AppendFormatted(_dir?.ToString() ?? "?");
		ImGui.TextUnformatted(text3);
		ImU8String text4 = new ImU8String(23, 1);
		text4.AppendLiteral("Tracked clone tethers: ");
		text4.AppendFormatted(_cloneTethers.Count);
		ImGui.TextUnformatted(text4);
		if (_dir.HasValue)
		{
			(int, Tether)? tuple2 = Resolve(_dir.Value);
			if (tuple2.HasValue)
			{
				ImU8String text5 = new ImU8String(15, 2);
				text5.AppendLiteral("Group ");
				text5.AppendFormatted((tuple2.Value.Item1 == 0) ? 1 : 2);
				text5.AppendLiteral("  tether ");
				text5.AppendFormatted(tuple2.Value.Item2);
				ImGui.TextUnformatted(text5);
			}
		}
	}

	private static Vector2 V(float x, float y)
	{
		return new Vector2(x, y);
	}

	private static Dictionary<Tether, Vector2> D((Tether t, Vector2 p) a, (Tether t, Vector2 p) b, (Tether t, Vector2 p) c, (Tether t, Vector2 p) d)
	{
		Dictionary<Tether, Vector2> dictionary = new Dictionary<Tether, Vector2>();
		dictionary[a.t] = a.p;
		dictionary[b.t] = b.p;
		dictionary[c.t] = c.p;
		dictionary[d.t] = d.p;
		return dictionary;
	}

	private static Dictionary<Strat, Preset> BuildPresets()
	{
		Dictionary<Strat, Preset> dictionary = new Dictionary<Strat, Preset>();
		Preset preset = new Preset
		{
			DifferentNetherwrath = true,
			North = Dir8.N
		};
		preset.A.Dirs = new Dir8[4]
		{
			Dir8.N,
			Dir8.NE,
			Dir8.E,
			Dir8.SE
		};
		preset.A.Tethers = new Tether[4]
		{
			Tether.Boss,
			Tether.Fan,
			Tether.Stack,
			Tether.Defamation
		};
		preset.A.Phase[1] = D((t: Tether.Stack, p: V(107.196f, 82.289f)), (t: Tether.Fan, p: V(102.328f, 81.136f)), (t: Tether.Defamation, p: V(118.145f, 107.307f)), (t: Tether.Boss, p: V(99.918f, 89.278f)));
		preset.A.Phase[2] = D((t: Tether.Stack, p: V(105.4f, 94.6f)), (t: Tether.Fan, p: V(102.8f, 92f)), (t: Tether.Defamation, p: V(105.4f, 94.6f)), (t: Tether.Boss, p: V(105.4f, 94.6f)));
		preset.A.Phase[3] = D((t: Tether.Stack, p: V(107.148f, 82.253f)), (t: Tether.Fan, p: V(102.374f, 81.15f)), (t: Tether.Defamation, p: V(104.5f, 91f)), (t: Tether.Boss, p: V(108f, 91f)));
		preset.A.Phase3Far = D((t: Tether.Stack, p: V(107.148f, 82.253f)), (t: Tether.Fan, p: V(102.374f, 81.15f)), (t: Tether.Defamation, p: V(113.65f, 89.192f)), (t: Tether.Boss, p: V(110.264f, 89.069f)));
		preset.A.Phase[4] = D((t: Tether.Boss, p: V(113.5f, 100f)), (t: Tether.Fan, p: V(113.5f, 100f)), (t: Tether.Defamation, p: V(113.5f, 100f)), (t: Tether.Stack, p: V(113.5f, 100f)));
		preset.A.Phase[5] = D((t: Tether.Fan, p: V(110f, 87.5f)), (t: Tether.Defamation, p: V(110f, 87.5f)), (t: Tether.Stack, p: V(110f, 87.5f)), (t: Tether.Boss, p: V(110f, 87.5f)));
		preset.A.PhaseRanged[4] = D((t: Tether.Boss, p: V(86.5f, 100f)), (t: Tether.Fan, p: V(86.5f, 100f)), (t: Tether.Defamation, p: V(86.5f, 100f)), (t: Tether.Stack, p: V(86.5f, 100f)));
		preset.A.PhaseRanged[5] = D((t: Tether.Fan, p: V(90f, 87.5f)), (t: Tether.Defamation, p: V(90f, 87.5f)), (t: Tether.Stack, p: V(90f, 87.5f)), (t: Tether.Boss, p: V(90f, 87.5f)));
		preset.B.Dirs = new Dir8[4]
		{
			Dir8.NW,
			Dir8.W,
			Dir8.SW,
			Dir8.S
		};
		preset.B.Tethers = new Tether[4]
		{
			Tether.Fan,
			Tether.Stack,
			Tether.Defamation,
			Tether.Nothing
		};
		preset.B.Phase[1] = D((t: Tether.Stack, p: V(92.6f, 82.088f)), (t: Tether.Fan, p: V(97.359f, 81.348f)), (t: Tether.Defamation, p: V(82.499f, 108.39f)), (t: Tether.Nothing, p: V(100.337f, 119.414f)));
		preset.B.Phase[2] = D((t: Tether.Stack, p: V(94.5f, 94.6f)), (t: Tether.Fan, p: V(96.8f, 92f)), (t: Tether.Defamation, p: V(94.5f, 94.6f)), (t: Tether.Nothing, p: V(94.5f, 94.6f)));
		preset.B.Phase[3] = D((t: Tether.Stack, p: V(92.572f, 82.14f)), (t: Tether.Fan, p: V(97.499f, 81.462f)), (t: Tether.Defamation, p: V(95.5f, 91f)), (t: Tether.Nothing, p: V(92f, 91f)));
		preset.B.Phase3Far = D((t: Tether.Stack, p: V(92.572f, 82.14f)), (t: Tether.Fan, p: V(97.499f, 81.462f)), (t: Tether.Defamation, p: V(86.323f, 89.144f)), (t: Tether.Nothing, p: V(89.569f, 89.074f)));
		preset.B.Phase[4] = D((t: Tether.Nothing, p: V(113.5f, 100f)), (t: Tether.Fan, p: V(113.5f, 100f)), (t: Tether.Defamation, p: V(113.5f, 100f)), (t: Tether.Stack, p: V(113.5f, 100f)));
		preset.B.Phase[5] = D((t: Tether.Nothing, p: V(110f, 87.5f)), (t: Tether.Fan, p: V(110f, 87.5f)), (t: Tether.Defamation, p: V(110f, 87.5f)), (t: Tether.Stack, p: V(110f, 87.5f)));
		preset.B.PhaseRanged[4] = D((t: Tether.Nothing, p: V(86.5f, 100f)), (t: Tether.Fan, p: V(86.5f, 100f)), (t: Tether.Defamation, p: V(86.5f, 100f)), (t: Tether.Stack, p: V(86.5f, 100f)));
		preset.B.PhaseRanged[5] = D((t: Tether.Nothing, p: V(90f, 87.5f)), (t: Tether.Fan, p: V(90f, 87.5f)), (t: Tether.Defamation, p: V(90f, 87.5f)), (t: Tether.Stack, p: V(90f, 87.5f)));
		dictionary[Strat.EU] = preset;
		Preset preset2 = new Preset
		{
			DifferentNetherwrath = false,
			North = Dir8.E
		};
		preset2.A.Dirs = new Dir8[4]
		{
			Dir8.SW,
			Dir8.S,
			Dir8.SE,
			Dir8.E
		};
		preset2.A.Tethers = new Tether[4]
		{
			Tether.Defamation,
			Tether.Fan,
			Tether.Stack,
			Tether.Boss
		};
		preset2.A.Phase[1] = D((t: Tether.Stack, p: V(119f, 104f)), (t: Tether.Fan, p: V(117.5f, 108f)), (t: Tether.Defamation, p: V(100f, 119.5f)), (t: Tether.Boss, p: V(113.75f, 100f)));
		preset2.A.Phase[2] = D((t: Tether.Stack, p: V(106f, 104.5f)), (t: Tether.Fan, p: V(108.5f, 102.5f)), (t: Tether.Defamation, p: V(106f, 104.5f)), (t: Tether.Boss, p: V(106f, 104.5f)));
		preset2.A.Phase[3] = D((t: Tether.Stack, p: V(115.8f, 102.5f)), (t: Tether.Fan, p: V(117.5f, 105.5f)), (t: Tether.Defamation, p: V(108f, 99.5f)), (t: Tether.Boss, p: V(108f, 99.5f)));
		preset2.A.Phase[4] = D((t: Tether.Stack, p: V(119f, 104f)), (t: Tether.Fan, p: V(117.5f, 108f)), (t: Tether.Defamation, p: V(110f, 90f)), (t: Tether.Boss, p: V(110f, 90f)));
		preset2.A.Phase[5] = D((t: Tether.Stack, p: V(119f, 104f)), (t: Tether.Fan, p: V(117.5f, 108f)), (t: Tether.Defamation, p: V(110f, 110f)), (t: Tether.Boss, p: V(110f, 110f)));
		preset2.B.Dirs = new Dir8[4]
		{
			Dir8.W,
			Dir8.NW,
			Dir8.N,
			Dir8.NE
		};
		preset2.B.Tethers = new Tether[4]
		{
			Tether.Nothing,
			Tether.Defamation,
			Tether.Fan,
			Tether.Stack
		};
		preset2.B.Phase[1] = D((t: Tether.Stack, p: V(119f, 97.5f)), (t: Tether.Fan, p: V(118f, 93f)), (t: Tether.Defamation, p: V(100f, 80.5f)), (t: Tether.Nothing, p: V(80.5f, 100f)));
		preset2.B.Phase[2] = D((t: Tether.Stack, p: V(106f, 95.5f)), (t: Tether.Fan, p: V(108.5f, 97.5f)), (t: Tether.Defamation, p: V(106f, 95.5f)), (t: Tether.Nothing, p: V(106f, 95.5f)));
		preset2.B.Phase[3] = D((t: Tether.Stack, p: V(115.8f, 97.5f)), (t: Tether.Fan, p: V(117.5f, 94.5f)), (t: Tether.Defamation, p: V(108f, 99.5f)), (t: Tether.Nothing, p: V(108f, 99.5f)));
		preset2.B.Phase[4] = D((t: Tether.Stack, p: V(119f, 97.5f)), (t: Tether.Fan, p: V(118f, 93f)), (t: Tether.Defamation, p: V(110f, 90f)), (t: Tether.Nothing, p: V(110f, 90f)));
		preset2.B.Phase[5] = D((t: Tether.Stack, p: V(119f, 97.5f)), (t: Tether.Fan, p: V(118f, 93f)), (t: Tether.Defamation, p: V(110f, 110f)), (t: Tether.Nothing, p: V(110f, 110f)));
		dictionary[Strat.JP] = preset2;
		Preset preset3 = new Preset
		{
			DifferentNetherwrath = false,
			North = Dir8.W
		};
		preset3.A.Dirs = new Dir8[4]
		{
			Dir8.W,
			Dir8.NW,
			Dir8.N,
			Dir8.NE
		};
		preset3.A.Tethers = new Tether[4]
		{
			Tether.Boss,
			Tether.Stack,
			Tether.Fan,
			Tether.Defamation
		};
		preset3.A.Phase[1] = D((t: Tether.Stack, p: V(81f, 96f)), (t: Tether.Fan, p: V(82.5f, 92f)), (t: Tether.Defamation, p: V(100f, 80.5f)), (t: Tether.Boss, p: V(89f, 100f)));
		preset3.A.Phase[2] = D((t: Tether.Stack, p: V(94f, 94.5f)), (t: Tether.Fan, p: V(91.5f, 97.5f)), (t: Tether.Defamation, p: V(94f, 94.5f)), (t: Tether.Boss, p: V(94f, 104.5f)));
		preset3.A.Phase[3] = D((t: Tether.Stack, p: V(89f, 96.5f)), (t: Tether.Fan, p: V(89f, 91f)), (t: Tether.Defamation, p: V(82.5f, 100.5f)), (t: Tether.Boss, p: V(82.5f, 100.5f)));
		preset3.A.Phase[4] = D((t: Tether.Stack, p: V(81f, 96f)), (t: Tether.Fan, p: V(82.5f, 92f)), (t: Tether.Defamation, p: V(90f, 110f)), (t: Tether.Boss, p: V(90f, 110f)));
		preset3.A.Phase[5] = D((t: Tether.Stack, p: V(81f, 96f)), (t: Tether.Fan, p: V(82.5f, 92f)), (t: Tether.Defamation, p: V(90f, 90f)), (t: Tether.Boss, p: V(90f, 90f)));
		preset3.B.Dirs = new Dir8[4]
		{
			Dir8.SW,
			Dir8.S,
			Dir8.SE,
			Dir8.E
		};
		preset3.B.Tethers = new Tether[4]
		{
			Tether.Stack,
			Tether.Fan,
			Tether.Defamation,
			Tether.Nothing
		};
		preset3.B.Phase[1] = D((t: Tether.Stack, p: V(81f, 102.5f)), (t: Tether.Fan, p: V(82f, 107f)), (t: Tether.Defamation, p: V(100f, 119.5f)), (t: Tether.Nothing, p: V(119.5f, 100f)));
		preset3.B.Phase[2] = D((t: Tether.Stack, p: V(94f, 104.5f)), (t: Tether.Fan, p: V(91.5f, 102.5f)), (t: Tether.Defamation, p: V(94f, 104.5f)), (t: Tether.Nothing, p: V(94f, 94.5f)));
		preset3.B.Phase[3] = D((t: Tether.Stack, p: V(89f, 103.5f)), (t: Tether.Fan, p: V(89f, 109f)), (t: Tether.Defamation, p: V(82.5f, 100.5f)), (t: Tether.Nothing, p: V(82.5f, 100.5f)));
		preset3.B.Phase[4] = D((t: Tether.Stack, p: V(81f, 102.5f)), (t: Tether.Fan, p: V(82f, 107f)), (t: Tether.Defamation, p: V(90f, 110f)), (t: Tether.Nothing, p: V(90f, 110f)));
		preset3.B.Phase[5] = D((t: Tether.Stack, p: V(81f, 102.5f)), (t: Tether.Fan, p: V(82f, 107f)), (t: Tether.Defamation, p: V(90f, 90f)), (t: Tether.Nothing, p: V(90f, 90f)));
		dictionary[Strat.Banana] = preset3;
		return dictionary;
	}
}

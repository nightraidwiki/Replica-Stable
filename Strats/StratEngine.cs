using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Plugin.Services;
using Replica.Logging;
using Replica.QuickDraws;

namespace Replica.Strats;

public sealed class StratEngine
{
	private readonly struct Pending(DateTime when, StratSlide slide, RoleSpot spot, LogEvent e)
	{
		public DateTime When { get; } = when;

		public StratSlide Slide { get; } = slide;

		public RoleSpot Spot { get; } = spot;

		public LogEvent Event { get; } = e;
	}

	private const double SuppressSeconds = 2.5;

	private readonly Configuration _config;

	private readonly QuickDrawEngine _draw;

	private readonly IPluginLog _log;

	private readonly CombatLogCapture _capture;

	private readonly Dictionary<string, DateTime> _lastFire = new Dictionary<string, DateTime>();

	private readonly Dictionary<string, string> _manualBranch = new Dictionary<string, string>();

	private readonly List<Pending> _pending = new List<Pending>();

	public StratEngine(Configuration config, QuickDrawEngine draw, IPluginLog log, CombatLogCapture capture)
	{
		_config = config;
		_draw = draw;
		_log = log;
		_capture = capture;
	}

	public void SetManualBranch(string slideId, string branchId)
	{
		_manualBranch[slideId] = branchId;
	}

	public string? GetManualBranch(string slideId)
	{
		if (!_manualBranch.TryGetValue(slideId, out string value))
		{
			return null;
		}
		return value;
	}

	public StratPack? ActivePack()
	{
		if (!_config.StratsEnabled)
		{
			return null;
		}
		uint terr = Plugin.ClientState.TerritoryType;
		List<StratPack> list = _config.StratPacks.FindAll((StratPack p) => p.Enabled && p.Territory == terr);
		if (list.Count == 0)
		{
			return null;
		}
		if (_config.SelectedStrat.TryGetValue(terr.ToString(), out string id))
		{
			StratPack stratPack = list.Find((StratPack p) => p.Id == id);
			if (stratPack != null)
			{
				return stratPack;
			}
		}
		return list[0];
	}

	public void Handle(LogEvent e)
	{
		StratPack stratPack = ActivePack();
		if (stratPack == null)
		{
			return;
		}
		foreach (StratSlide slide in stratPack.Slides)
		{
			if (!SlideMatches(slide, e) || (_lastFire.TryGetValue(slide.Id, out var value) && (DateTime.Now - value).TotalSeconds < 2.5))
			{
				continue;
			}
			_lastFire[slide.Id] = DateTime.Now;
			StratBranch stratBranch = ResolveBranch(slide, e);
			if (stratBranch == null)
			{
				continue;
			}
			RoleSpot roleSpot = stratBranch.Spots.Find((RoleSpot s) => s.Enabled && s.Role == _config.MyRole);
			if (roleSpot != null)
			{
				if (slide.DelaySeconds > 0.01f)
				{
					_pending.Add(new Pending(DateTime.Now.AddSeconds(slide.DelaySeconds), slide, roleSpot, e));
				}
				else
				{
					FireSpot(slide, roleSpot, e);
				}
			}
		}
	}

	public void Tick()
	{
		if (_pending.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		for (int num = _pending.Count - 1; num >= 0; num--)
		{
			if (!(_pending[num].When > now))
			{
				Pending pending = _pending[num];
				_pending.RemoveAt(num);
				FireSpot(pending.Slide, pending.Spot, pending.Event);
			}
		}
	}

	public void Preview(StratSlide slide, StratBranch branch)
	{
		RoleSpot roleSpot = branch.Spots.Find((RoleSpot s) => s.Enabled && s.Role == _config.MyRole) ?? ((branch.Spots.Count > 0) ? branch.Spots[0] : null);
		if (roleSpot != null)
		{
			FireSpot(slide, roleSpot, new LogEvent
			{
				Name = "preview"
			}, preview: true);
		}
	}

	private void FireSpot(StratSlide slide, RoleSpot spot, LogEvent e, bool preview = false)
	{
		string ownerId = "strat:" + slide.Id + ":marker";
		string ownerId2 = "strat:" + slide.Id + ":leash";
		_draw.ClearExternal(ownerId);
		_draw.ClearExternal(ownerId2);
		bool flag = spot.Anchor == SpotAnchor.TetheredToMe;
		DrawSpec d = new DrawSpec
		{
			Shape = spot.Shape,
			Color = spot.Color,
			Radius = spot.Radius,
			InnerRadius = MathF.Max(0.1f, spot.Radius - 1f),
			HalfWidth = MathF.Max(0.5f, spot.Radius),
			Anchor = (flag ? DrawAnchor.TetheredToMe : DrawAnchor.FixedPosition),
			AttachToActor = flag,
			FixedPosition = spot.Position,
			TetherFilterId = spot.TetherId,
			Duration = spot.Duration,
			UseEventDuration = false
		};
		_draw.SpawnExternal(ownerId, d, e, preview);
		if (spot.ShowLeash)
		{
			DrawSpec d2 = new DrawSpec
			{
				Shape = QuickShape.ChevronPath,
				Color = spot.LeashColor,
				ChevronSpacing = 2.5f,
				LineThickness = 4f,
				Anchor = DrawAnchor.Self,
				AttachToActor = true,
				Link = (flag ? LinkTarget.TetheredToMe : LinkTarget.FixedSpot),
				LinkPosition = spot.Position,
				TetherFilterId = spot.TetherId,
				Duration = spot.Duration
			};
			_draw.SpawnExternal(ownerId2, d2, e, preview);
		}
	}

	private StratBranch? ResolveBranch(StratSlide slide, LogEvent e)
	{
		if (slide.Branches.Count == 0)
		{
			return null;
		}
		if (slide.Branches.Count == 1)
		{
			return slide.Branches[0];
		}
		StratBranch stratBranch = null;
		foreach (StratBranch branch in slide.Branches)
		{
			if (branch.Conditions.Count > 0)
			{
				if (EvalBranch(branch, e))
				{
					return branch;
				}
				continue;
			}
			switch (branch.Detect)
			{
			case BranchDetect.MyStatus:
				if (branch.StatusId != 0 && SelfHasStatus(branch.StatusId))
				{
					return branch;
				}
				break;
			case BranchDetect.BossPosition:
			{
				Compass? compass = BossSideFor(branch.BossId, e);
				if (compass.HasValue && branch.BossSide == compass.Value)
				{
					return branch;
				}
				break;
			}
			default:
				if (stratBranch == null)
				{
					stratBranch = branch;
				}
				break;
			}
		}
		if (_manualBranch.TryGetValue(slide.Id, out string bid))
		{
			StratBranch stratBranch2 = slide.Branches.Find((StratBranch b) => b.Id == bid);
			if (stratBranch2 != null)
			{
				return stratBranch2;
			}
		}
		return stratBranch ?? slide.Branches[0];
	}

	private bool EvalBranch(StratBranch b, LogEvent e)
	{
		if (b.RequireAll)
		{
			foreach (StratCondition condition in b.Conditions)
			{
				if (!EvalCondition(condition, e))
				{
					return false;
				}
			}
			return true;
		}
		foreach (StratCondition condition2 in b.Conditions)
		{
			if (EvalCondition(condition2, e))
			{
				return true;
			}
		}
		return false;
	}

	private bool EvalCondition(StratCondition c, LogEvent e)
	{
		bool flag;
		switch (c.Kind)
		{
		case CondKind.MyStatus:
			flag = c.StatusId != 0 && SelfHasStatus(c.StatusId);
			break;
		case CondKind.MyRole:
			flag = SelfRoleMatches(c.Role);
			break;
		case CondKind.BossSide:
		{
			Compass? compass = BossSideFor(c.BossId, e);
			int num;
			if (compass.HasValue)
			{
				Compass valueOrDefault = compass.GetValueOrDefault();
				num = ((valueOrDefault == c.BossSide) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			flag = (byte)num != 0;
			break;
		}
		case CondKind.TetherOnMe:
			flag = SelfHasTether(c.TetherId);
			break;
		default:
			flag = true;
			break;
		}
		bool flag2 = flag;
		if (!c.Negate)
		{
			return flag2;
		}
		return !flag2;
	}

	private static bool SelfHasStatus(uint statusId)
	{
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		foreach (IStatus status in localPlayer.StatusList)
		{
			if (status != null && status.StatusId == statusId)
			{
				return true;
			}
		}
		return false;
	}

	private static bool SelfRoleMatches(RoleCat want)
	{
		byte valueOrDefault = (Plugin.ObjectTable.LocalPlayer?.ClassJob.ValueNullable?.Role).GetValueOrDefault();
		return want switch
		{
			RoleCat.Tank => valueOrDefault == 1, 
			RoleCat.Healer => valueOrDefault == 4, 
			RoleCat.Melee => valueOrDefault == 2, 
			RoleCat.Ranged => valueOrDefault == 3, 
			RoleCat.Dps => (uint)(valueOrDefault - 2) <= 1u, 
			_ => false, 
		};
	}

	private bool SelfHasTether(uint tetherId)
	{
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		uint entityId = localPlayer.EntityId;
		foreach (CombatLogCapture.LiveTether activeTether in _capture.ActiveTethers)
		{
			if ((tetherId == 0 || activeTether.Id == tetherId) && (activeTether.From == entityId || activeTether.To == entityId))
			{
				return true;
			}
		}
		return false;
	}

	private static Compass? BossSideFor(uint bossId, LogEvent e)
	{
		IGameObject gameObject = null;
		if (bossId != 0)
		{
			gameObject = Plugin.ObjectTable.SearchById(bossId);
		}
		if (gameObject == null && e.SourceId != 0)
		{
			gameObject = Plugin.ObjectTable.SearchById(e.SourceId);
		}
		if (gameObject == null)
		{
			return null;
		}
		float num = gameObject.Position.X - 100f;
		float num2 = gameObject.Position.Z - 100f;
		if (MathF.Abs(num) < 0.5f && MathF.Abs(num2) < 0.5f)
		{
			return null;
		}
		float num3 = MathF.Atan2(num, 0f - num2) * (180f / (float)Math.PI);
		if (num3 < 0f)
		{
			num3 += 360f;
		}
		return (Compass)((int)MathF.Round(num3 / 45f) & 7);
	}

	private static bool SlideMatches(StratSlide slide, LogEvent e)
	{
		bool flag;
		switch (slide.On)
		{
		case TriggerMatch.Any:
			flag = true;
			break;
		case TriggerMatch.Cast:
			flag = e.Kind == LogKind.CastStart;
			break;
		case TriggerMatch.CastEnd:
		{
			LogKind kind = e.Kind;
			bool flag2 = ((kind == LogKind.CastFinish || kind == LogKind.Ability) ? true : false);
			flag = flag2;
			break;
		}
		case TriggerMatch.StatusGain:
			flag = e.Kind == LogKind.StatusGain;
			break;
		case TriggerMatch.StatusLose:
			flag = e.Kind == LogKind.StatusLose;
			break;
		case TriggerMatch.Death:
			flag = e.Kind == LogKind.Death;
			break;
		case TriggerMatch.Headmarker:
			flag = e.Kind == LogKind.Headmarker;
			break;
		case TriggerMatch.Tether:
			flag = e.Kind == LogKind.Tether;
			break;
		case TriggerMatch.Chat:
			flag = e.Kind == LogKind.Chat;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return false;
		}
		if (slide.MatchById)
		{
			return e.DataId == slide.DataId;
		}
		if (string.IsNullOrEmpty(slide.Pattern))
		{
			return true;
		}
		return e.Name.Contains(slide.Pattern, StringComparison.OrdinalIgnoreCase);
	}
}

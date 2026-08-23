using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using Lumina.Excel.Sheets;
using Replica.Engine.Helper;
using Replica.Engine.Interop.ActionEffect;
using Replica.Engine.Managers;
using Replica.Engine.Memory;
using Replica.Engine.Module;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;

namespace Replica.Engine;

public sealed class FightModuleHost : IDisposable
{
	public sealed record MechView(string Key, string Display, uint Phase, bool HasConfig = false, System.Action? DrawConfig = null);

	private sealed class FightPack
	{
		public required BaseModule Host { get; init; }

		public required string Name { get; init; }

		public required string Display { get; init; }

		public required Category Category { get; init; }

		public required uint Cfc { get; init; }

		public required uint Territory { get; init; }

		public required List<ISpecialAction> Actions { get; init; }

		public required List<MechView> Mechanics { get; init; }
	}

	public sealed class FightView
	{
		public required string Key { get; init; }

		public required string Display { get; init; }

		public required Category Category { get; init; }

		public required uint Cfc { get; init; }

		public required uint Territory { get; init; }

		public required IReadOnlyList<MechView> Mechanics { get; init; }

		public required bool UseAutoDraw { get; init; }

		public required bool IsActive { get; init; }

		public required bool HasConfig { get; init; }

		public System.Action? DrawConfig { get; init; }
	}

	private readonly IPluginLog _log;

	private readonly CombatLogCapture? _capture;

	private readonly List<FightPack> _packs = new List<FightPack>();

	private readonly ResourceService? _resourceService;

	private FightPack? _active;

	private bool _hooksReady;

	private uint _lastTerritory;

	private uint _lastWeather;

	private bool _lastInCombat;

	private bool _lastForceUmad;

	private DateTime _allDeadSince = DateTime.MinValue;

	private bool _fallbackWiped;

	private static readonly TimeSpan FallbackWipeHold = TimeSpan.FromSeconds(1.5);

	private const string UmadFightKey = "DancingMad";

	public IReadOnlyList<FightView> Fights => _packs.Select((FightPack p) => new FightView
	{
		Key = p.Name,
		Display = p.Display,
		Category = p.Category,
		Cfc = p.Cfc,
		Territory = p.Territory,
		Mechanics = p.Mechanics,
		UseAutoDraw = p.Host.UseAutoDraw,
		IsActive = (p.Territory != 0 && p.Territory == Plugin.ClientState.TerritoryType),
		HasConfig = p.Host.HasConfig,
		DrawConfig = (p.Host.HasConfig ? new System.Action(p.Host.DrawConfig) : null)
	}).ToList();

	private static bool ForceUmadActive => Plugin.ConfigStatic?.ForceUmadActive ?? false;

	public bool UmadForced
	{
		get
		{
			if (ForceUmadActive)
			{
				return _active?.Name == "DancingMad";
			}
			return false;
		}
	}

	public bool HooksInstalled => _hooksReady;

	public int ModuleCount => _active?.Actions.Count ?? _packs.Sum((FightPack p) => p.Actions.Count);

	public uint TerritoryId => _active?.Territory ?? 0;

	public string FightName
	{
		get
		{
			if (_active != null)
			{
				if (!UmadForced)
				{
					return _active.Name;
				}
				return _active.Name + " (forced)";
			}
			return "none";
		}
	}

	private static bool MasterOff
	{
		get
		{
			Configuration configStatic = Plugin.ConfigStatic;
			if (configStatic != null)
			{
				return !configStatic.ModulesEnabled;
			}
			return false;
		}
	}

	private bool InZone
	{
		get
		{
			if (_active == null)
			{
				return false;
			}
			if (UmadForced)
			{
				return true;
			}
			if (_active.Territory != 0)
			{
				return Plugin.ClientState.TerritoryType == _active.Territory;
			}
			return true;
		}
	}

	private IEnumerable<ISpecialAction> Actions => _active.Actions.Where((ISpecialAction a) => !MechanicDisabled(_active.Name, a.Name)).Where(WeatherOk);

	public FightModuleHost(IPluginLog log, CombatLogCapture? capture = null)
	{
		_log = log;
		_capture = capture;
		foreach (ModuleRegistry.LoadedFight item in ModuleRegistry.LoadAll())
		{
			RegisterFight(item.Host, item.Mechanics);
		}
		_log.Information($"[Replica] loaded {_packs.Count} fight packs, {_packs.Sum((FightPack p) => p.Actions.Count)} mechanics");
		try
		{
			ClientOmenHooks.Init();
			_hooksReady = true;
		}
		catch (Exception exception)
		{
			_hooksReady = false;
			_log.Error(exception, "[Replica] ClientOmenHooks.Init failed; omen drawing disabled");
		}
		try
		{
			_resourceService = new ResourceService();
			_resourceService.Init();
		}
		catch (Exception exception2)
		{
			_resourceService = null;
			_log.Error(exception2, "[Replica] ResourceService.Init failed; omen blocking disabled");
		}
	}

	public void SetResourceHook(bool enabled)
	{
		_resourceService?.SetEnabled(enabled);
	}

	private void RegisterFight(BaseModule meta, IEnumerable<ISpecialAction> actions)
	{
		List<ISpecialAction> list = actions.ToList();
		string name = meta.GetType().Name;
		_packs.Add(new FightPack
		{
			Host = meta,
			Name = name,
			Display = FightDisplayNames.For(name),
			Category = meta.ModuleInfo.Category,
			Cfc = meta.ModuleInfo.GroupID,
			Territory = ResolveTerritory(meta.ModuleInfo.GroupID),
			Actions = list,
			Mechanics = list.Select((ISpecialAction a) => new MechView(a.Name ?? string.Empty, ResolveMechName(a), a.Phase, a.HasConfig, a.DrawConfig)).ToList()
		});
	}

	private static bool HasCjk(string s)
	{
		foreach (char c in s)
		{
			if (c >= '一' && c <= '鿿')
			{
				return true;
			}
		}
		return false;
	}

	private static string ResolveMechName(ISpecialAction a)
	{
		string text = a.Name ?? string.Empty;
		if (!HasCjk(text))
		{
			return text;
		}
		foreach (uint item in a.ActionID)
		{
			if (item == 0)
			{
				continue;
			}
			try
			{
				Lumina.Excel.Sheets.Action? rowOrDefault = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English).GetRowOrDefault(item);
				if (rowOrDefault.HasValue)
				{
					string text2 = rowOrDefault.GetValueOrDefault().Name.ExtractText();
					if (!string.IsNullOrWhiteSpace(text2))
					{
						return text2;
					}
				}
			}
			catch
			{
			}
		}
		return text;
	}

	private static bool FightDisabled(string key)
	{
		return Plugin.ConfigStatic?.DisabledFights.Contains(key) ?? false;
	}

	private static bool MechanicDisabled(string fightKey, string mech)
	{
		return Plugin.ConfigStatic?.DisabledMechanics.Contains(fightKey + "/" + mech) ?? false;
	}

	private uint ResolveTerritory(uint cfcId)
	{
		try
		{
			ContentFinderCondition? rowOrDefault = Plugin.DataManager.GetExcelSheet<ContentFinderCondition>().GetRowOrDefault(cfcId);
			if (rowOrDefault.HasValue)
			{
				uint rowId = rowOrDefault.GetValueOrDefault().TerritoryType.RowId;
				if (rowId != 0)
				{
					return rowId;
				}
			}
		}
		catch (Exception ex)
		{
			_log.Warning("[Replica] territory resolve failed: " + ex.Message);
		}
		return 0u;
	}

	private void ResolveActive()
	{
		uint territory = Plugin.ClientState.TerritoryType;
		FightPack fightPack = _packs.FirstOrDefault((FightPack p) => p.Territory != 0 && p.Territory == territory);
		if (fightPack != null)
		{
			_active = fightPack;
		}
		else if (ForceUmadActive)
		{
			_active = _packs.FirstOrDefault((FightPack p) => p.Name == "DancingMad");
		}
		else
		{
			_active = null;
		}
	}

	public unsafe void Tick()
	{
		uint num = 0u;
		try
		{
			EnvManager* ptr = EnvManager.Instance();
			if (ptr != null)
			{
				num = ptr->ActiveWeather;
			}
		}
		catch
		{
		}
		uint territoryType = Plugin.ClientState.TerritoryType;
		bool forceUmadActive = ForceUmadActive;
		if (territoryType != _lastTerritory)
		{
			_lastTerritory = territoryType;
			_lastWeather = num;
			_lastForceUmad = forceUmadActive;
			ResolveActive();
			ResetAll();
		}
		else if (forceUmadActive != _lastForceUmad)
		{
			_lastForceUmad = forceUmadActive;
			ResolveActive();
			if (forceUmadActive)
			{
				ResetAll();
			}
		}
		FightRuntime.SetWeather(num);
		if (num != _lastWeather)
		{
			uint lastWeather = _lastWeather;
			_lastWeather = num;
			DispatchWeatherChange(lastWeather, num);
		}
		bool flag = Plugin.Condition[ConditionFlag.InCombat];
		if (flag && !_lastInCombat)
		{
			ResetAll();
		}
		if (!flag && _lastInCombat && !UmadForced)
		{
			CleanVfx();
		}
		_lastInCombat = flag;
		if (!Plugin.DutyState.IsDutyStarted)
		{
			if (PartyAllDead())
			{
				if (_allDeadSince == DateTime.MinValue)
				{
					_allDeadSince = DateTime.Now;
				}
				else if (!_fallbackWiped && DateTime.Now - _allDeadSince >= FallbackWipeHold)
				{
					_fallbackWiped = true;
					if (_active != null)
					{
						WipeCleanup();
					}
				}
			}
			else
			{
				_allDeadSince = DateTime.MinValue;
				_fallbackWiped = false;
			}
		}
		else
		{
			_allDeadSince = DateTime.MinValue;
			_fallbackWiped = false;
		}
		if (_active == null)
		{
			VfxBlocker.ClearSyncedBlocks();
		}
		else
		{
			bool flag2 = MasterOff || FightDisabled(_active.Name);
			if (flag2)
			{
				VfxBlocker.ClearSyncedBlocks();
			}
			else
			{
				VfxBlocker.SyncOmenBlocks(new Dictionary<uint, HashSet<uint>>[1] { _active.Host.BlockOmenMap }, new Dictionary<uint, HashSet<string>>[1] { _active.Host.BlockOmenPathMap }, FightRuntime.WeatherId, _active.Host.ModuleInfo.GroupID);
			}
			foreach (ISpecialAction action in _active.Actions)
			{
				try
				{
					if (flag2 || MechanicDisabled(_active.Name, action.Name) || !WeatherOk(action))
					{
						foreach (StaticVfx aoe in action.aoes)
						{
							if (aoe != null)
							{
								aoe.Enable = false;
							}
						}
						continue;
					}
					List<StaticVfx> list = action.ActiveAOEs.ToList();
					List<StaticVfx> list2 = action.aoes.ToList();
					if (list.Count > 0 && list2.Count > 0)
					{
						foreach (StaticVfx item in list2)
						{
							if (item != null)
							{
								item.Enable = list.Contains(item);
							}
						}
					}
					action.Update();
				}
				catch (Exception ex)
				{
					_log.Debug("[Replica] module update: " + ex.Message);
				}
			}
		}
		try
		{
			FrameworkUpdateManager.Tick();
		}
		catch (Exception ex2)
		{
			_log.Debug("[Replica] tick: " + ex2.Message);
		}
	}

	private static bool WeatherOk(ISpecialAction a)
	{
		if (a.WeatherID != 0)
		{
			return a.WeatherID == FightRuntime.WeatherId;
		}
		return true;
	}

	public void HandleDutyWipe()
	{
		WipeCleanup();
	}

	private void WipeCleanup()
	{
		CleanVfx();
		ResetAll();
	}

	private static bool PartyAllDead()
	{
		bool result = false;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			if (allPlayer is IBattleChara battleChara)
			{
				result = true;
				if (!battleChara.IsDead && battleChara.CurrentHp != 0)
				{
					return false;
				}
			}
		}
		return result;
	}

	private void ResetAll()
	{
		VfxBlocker.ClearSyncedBlocks();
		_capture?.ResetLiveState();
		FightClientState.ClearEnmity();
		Data.Clear();
		foreach (FightPack pack in _packs)
		{
			foreach (ISpecialAction action in pack.Actions)
			{
				try
				{
					action.Reset();
				}
				catch (Exception ex)
				{
					_log.Debug("[Replica] reset: " + ex.Message);
				}
			}
		}
	}

	public void CleanVfx()
	{
		try
		{
			ClientOmenHooks.CleanAllVfx();
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] cleanvfx: " + ex.Message);
		}
		foreach (FightPack pack in _packs)
		{
			foreach (ISpecialAction action in pack.Actions)
			{
				try
				{
					action.Reset();
				}
				catch (Exception ex2)
				{
					_log.Debug("[Replica] cleanvfx reset: " + ex2.Message);
				}
			}
		}
	}

	public void OnEvent(LogEvent e)
	{
		if (!_hooksReady)
		{
			return;
		}
		ResolveActive();
		if (!InZone || _active == null || MasterOff || FightDisabled(_active.Name))
		{
			return;
		}
		try
		{
			switch (e.Kind)
			{
			case Replica.Logging.LogKind.CastStart:
				HandleCast(e);
				break;
			case Replica.Logging.LogKind.Ability:
				HandleAbility(e);
				break;
			case Replica.Logging.LogKind.StatusGain:
				HandleStatus(e);
				break;
			case Replica.Logging.LogKind.StatusLose:
				HandleStatusRemove(e);
				break;
			case Replica.Logging.LogKind.MapEffect:
				HandleMapEffect(e);
				break;
			case Replica.Logging.LogKind.TimelineEvent:
				HandleTimeline(e);
				break;
			case Replica.Logging.LogKind.TimelineSync:
				HandleTimelineSync(e);
				break;
			case Replica.Logging.LogKind.Headmarker:
				HandleHeadmarker(e);
				break;
			case Replica.Logging.LogKind.Added:
				HandleAdded(e);
				break;
			case Replica.Logging.LogKind.Tether:
				HandleTether(e);
				break;
			case Replica.Logging.LogKind.TetherCancel:
				HandleTetherCancel(e);
				break;
			case Replica.Logging.LogKind.EventObject:
				HandleEventObject(e);
				break;
			case Replica.Logging.LogKind.ActorControl:
				HandleActorControl(e);
				break;
			case Replica.Logging.LogKind.ActorTargetVfx:
				HandleActorTargetVfx(e);
				break;
			case Replica.Logging.LogKind.CastFinish:
			case Replica.Logging.LogKind.Death:
			case Replica.Logging.LogKind.AbilityExtra:
			case Replica.Logging.LogKind.Note:
			case Replica.Logging.LogKind.Chat:
			case Replica.Logging.LogKind.Vfx:
				break;
			}
		}
		catch (Exception ex)
		{
			_log.Debug($"[Replica] dispatch {e.Kind}: {ex.Message}");
		}
	}

	public void HandleChatMessage(uint chatType, string content)
	{
		if (!_hooksReady)
		{
			return;
		}
		ResolveActive();
		if (!InZone || _active == null || MasterOff || FightDisabled(_active.Name))
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			try
			{
				action.OnChatMessage(chatType, content);
			}
			catch (Exception ex)
			{
				_log.Debug("[Replica] chat dispatch: " + ex.Message);
			}
		}
	}

	public void HandleNpcYell(ulong sourceId, ushort message)
	{
		if (!_hooksReady)
		{
			return;
		}
		ResolveActive();
		if (!InZone || _active == null || MasterOff || FightDisabled(_active.Name))
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			try
			{
				action.OnNpcYell(sourceId, message);
			}
			catch (Exception ex)
			{
				_log.Debug("[Replica] npc-yell dispatch: " + ex.Message);
			}
		}
	}

	private void DispatchWeatherChange(uint oldWeather, uint newWeather)
	{
		if (!_hooksReady)
		{
			return;
		}
		ResolveActive();
		if (!InZone || _active == null || MasterOff || FightDisabled(_active.Name))
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			try
			{
				action.OnWeatherChange(oldWeather, newWeather);
			}
			catch (Exception ex)
			{
				_log.Debug("[Replica] weather dispatch: " + ex.Message);
			}
		}
	}

	private void HandleCast(LogEvent e)
	{
		Vector3 pos = (Data.LastCastPositions.TryGetValue(e.SourceId, out var value) ? value : new Vector3(e.X, 0f, e.Y));
		ActorCastInfo info = new ActorCastInfo
		{
			ActionId = (ushort)e.DataId,
			DisplayDelay = (byte)e.Param1,
			CastTime = e.Value,
			SourceId = e.SourceId,
			TargetId = e.TargetId,
			Facing = new Angle(e.Heading),
			Pos = pos
		};
		foreach (ISpecialAction action in Actions)
		{
			if (action.ActionID.Contains(e.DataId))
			{
				action.OnActionCast(info);
			}
		}
		if (_active.Host.UseAutoDraw)
		{
			AutoDrawModule.Run(info);
		}
	}

	private void HandleAbility(LogEvent e)
	{
		IGameObject source = Plugin.ObjectTable.SearchById(e.SourceId);
		IGameObject gameObject = ((e.TargetId != 0) ? Plugin.ObjectTable.SearchById(e.TargetId) : null);
		StaticVfx[] array = ClientOmenHooks.drawOmenElementList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnHitEvent(e.DataId, gameObject);
		}
		ActorVfx[] array2 = ClientOmenHooks.ActorVfxList.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].OnHitEvent(e.DataId, gameObject);
		}
		TargetEffect[] targetEffects = ((e.AbilityTargetIds.Length == 0) ? Array.Empty<TargetEffect>() : e.AbilityTargetIds.Select((uint id) => new TargetEffect
		{
			TargetID = id
		}).ToArray());
		ActorAbilityInfo info = new ActorAbilityInfo
		{
			ActionId = e.DataId,
			Source = source,
			Target = gameObject,
			TargetEffects = targetEffects,
			Rotation = new Angle(e.Heading),
			Pos = new Vector3(e.X, 0f, e.Y)
		};
		foreach (ISpecialAction action in Actions)
		{
			if (action.ActionID.Contains(e.DataId))
			{
				action.OnAbilityCast(info);
			}
		}
		foreach (ISpecialAction action2 in Actions)
		{
			action2.OnDrawQueue(info);
		}
	}

	private void HandleStatus(LogEvent e)
	{
		ActorStatusChangeInfo info = new ActorStatusChangeInfo
		{
			StatusID = e.DataId,
			Stack = e.Count,
			TargetID = e.TargetId,
			SourceID = e.SourceId,
			Time = e.Value
		};
		foreach (ISpecialAction action in Actions)
		{
			action.OnAddStatus(info);
		}
	}

	private void HandleStatusRemove(LogEvent e)
	{
		ActorStatusChangeInfo info = new ActorStatusChangeInfo
		{
			StatusID = e.DataId,
			Stack = e.Count,
			TargetID = e.TargetId,
			SourceID = e.SourceId,
			Time = e.Value
		};
		foreach (ISpecialAction action in Actions)
		{
			action.OnRemoveStatus(info);
		}
	}

	private void HandleMapEffect(LogEvent e)
	{
		byte b = (byte)e.Param1;
		uint category = e.Category;
		ushort a = (ushort)(category & 0xFFFF);
		ushort a2 = (ushort)(category >> 16);
		foreach (ISpecialAction action in Actions)
		{
			action.OnEnvControl(b, category);
			action.OnMapEffect(b, a, a2);
		}
	}

	private void HandleTimeline(LogEvent e)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(e.SourceId);
		if (gameObject == null)
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorPlayActionTimelineEvent(gameObject, e.DataId);
		}
	}

	private void HandleTimelineSync(LogEvent e)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(e.TargetId);
		if (gameObject == null)
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorPlayActionTimelineEvent(gameObject, e.DataId);
		}
	}

	private void HandleHeadmarker(LogEvent e)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(e.SourceId);
		if (gameObject == null)
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			action.OnTargetIconEvent(gameObject, e.DataId, e.Param1);
		}
	}

	private void HandleAdded(LogEvent e)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(e.SourceId);
		if (gameObject == null)
		{
			return;
		}
		foreach (ISpecialAction action in Actions)
		{
			action.OnObjectCreatedEvent(gameObject);
		}
	}

	private void HandleTether(LogEvent e)
	{
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorTetherEvent(e.SourceId, e.DataId, e.TargetId);
		}
	}

	private void HandleTetherCancel(LogEvent e)
	{
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorTetherCancelEvent(e.SourceId);
		}
	}

	private void HandleEventObject(LogEvent e)
	{
		ushort p = (ushort)e.Param1;
		ushort p2 = (ushort)e.Param2;
		foreach (ISpecialAction action in Actions)
		{
			action.OnEventObjectAnimation(e.SourceId, p, p2);
		}
	}

	private void HandleActorControl(LogEvent e)
	{
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorControl(e.SourceId, e.Category, e.Param1, e.Param2, e.Param3, e.Param4);
		}
	}

	private void HandleActorTargetVfx(LogEvent e)
	{
		foreach (ISpecialAction action in Actions)
		{
			action.OnActorTargetVfx(e.SourceId, e.DataId);
		}
	}

	public void Dispose()
	{
		try
		{
			ResetAll();
		}
		catch
		{
		}
		try
		{
			VfxBlocker.Dispose();
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] vfx blocker dispose: " + ex.Message);
		}
		try
		{
			_resourceService?.Dispose();
		}
		catch (Exception ex2)
		{
			_log.Debug("[Replica] resource service dispose: " + ex2.Message);
		}
		try
		{
			ClientOmenHooks.DisposeHooks();
		}
		catch (Exception ex3)
		{
			_log.Debug("[Replica] dispose: " + ex3.Message);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel.Sheets;
using Replica.Engine;
using Replica.Engine.Memory;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;
using Replica.QuickDraws;

namespace Replica.Logging;

public sealed class CombatLogCapture : IDisposable
{
	public readonly record struct LiveTether(uint From, uint To, ushort Id);

	public readonly record struct LiveHeadmarker(uint ActorId, uint IconId);

	public readonly struct ActorSample(uint id, ActorKind kind, float x, float z, float rot, float hpPct, uint castId)
	{
		public readonly uint Id = id;

		public readonly ActorKind Kind = kind;

		public readonly float X = x;

		public readonly float Z = z;

		public readonly float Rot = rot;

		public readonly float HpPct = hpPct;

		public readonly uint CastId = castId;
	}

	public sealed class MapFrame
	{
		public int Pull;

		public double T;

		public ActorSample[] Actors = Array.Empty<ActorSample>();

		public MapAoe[] Aoes = Array.Empty<MapAoe>();
	}

	private unsafe delegate void ActorCastDelegate(uint casterId, ActorCast* data);

	private unsafe delegate void ProcessActionEffectDelegate(uint casterEntityId, Character* casterPtr, FFXIVClientStructs.FFXIV.Common.Math.Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targetEntityIds);

	private delegate void ProcessActorControlDelegate(uint actorId, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, GameObjectId targetId, [MarshalAs(UnmanagedType.U1)] bool replaying);

	private unsafe delegate void ProcessMapEffectDelegate(void* self, uint index, ushort s1, ushort s2);

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct PlayActionTimelineSyncPacket
	{
		public unsafe fixed uint EntityIds[10];

		public unsafe fixed ushort TimelineIds[10];
	}

	private unsafe delegate void ProcessPlayActionTimelineSyncDelegate(PlayActionTimelineSyncPacket* data);

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct NpcYellPacket
	{
		public ulong SourceId;

		public int Padding;

		public ushort MessageId;
	}

	private unsafe delegate void ProcessNpcYellDelegate(NpcYellPacket* data);

	private delegate nint ActorVfxCreateDelegate(nint path, nint caster, nint target, float a4, char a5, ushort a6, char a7);

	public sealed class PullInfo
	{
		public int Index;

		public string Label = "";

		public DateTime Start;

		public DateTime End;

		public int Events;

		public uint Territory;

		public uint MapId;

		public string ZoneName = "";

		public string BossName = "";

		public string Duration()
		{
			TimeSpan timeSpan = ((End == DateTime.MinValue) ? DateTime.Now : End) - Start;
			if (timeSpan.TotalHours >= 1.0)
			{
				return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
			}
			return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
		}

		public string GetEffectiveZoneName()
		{
			if (!string.IsNullOrWhiteSpace(ZoneName))
			{
				return ZoneName;
			}
			if (Territory != 0)
			{
				return ZoneLibrary.NameOf(Territory);
			}
			return "Open World";
		}

		public string GetEffectiveBossName()
		{
			if (!string.IsNullOrWhiteSpace(BossName) && BossName != "(none)" && BossName != "none")
			{
				return BossName;
			}
			return "";
		}

		public string GetFullDisplayLabel()
		{
			string zone = GetEffectiveZoneName();
			string boss = GetEffectiveBossName();
			string dur = Duration();
			if (!string.IsNullOrWhiteSpace(boss))
			{
				return $"{zone} · {boss} · {dur} · Pull {Index}";
			}
			return $"{zone} · {dur} · Pull {Index}";
		}

		public string GetFileSlug()
		{
			string zone = SanitizeForFileName(GetEffectiveZoneName());
			string boss = SanitizeForFileName(GetEffectiveBossName());
			string dur = Duration().Replace(":", "m") + "s";
			if (!string.IsNullOrWhiteSpace(boss) && boss != "Unknown" && boss != "none")
			{
				return $"{zone}_{boss}_{dur}_pull{Index}";
			}
			return $"{zone}_{dur}_pull{Index}";
		}
	}

	private sealed class ActorState
	{
		public uint LastCastId;

		public HashSet<(uint id, uint src)> Statuses = new HashSet<(uint, uint)>();

		public bool Alive = true;

		public bool SeenThisPass;
	}

	private readonly Configuration _config;

	private readonly IPluginLog _log;

	private readonly Dictionary<uint, ActorState> _actors = new Dictionary<uint, ActorState>();

	private readonly HashSet<uint> _eventObjs = new HashSet<uint>();

	private readonly HashSet<uint> _eventObjScratch = new HashSet<uint>();

	private readonly HashSet<(uint id, uint src)> _statusScratch = new HashSet<(uint, uint)>();

	private readonly List<LogEvent> _events = new List<LogEvent>();

	private long _seq;

	private readonly Queue<(LogEvent e, bool addToLog)> _hookQueue = new Queue<(LogEvent, bool)>();

	private readonly object _hookQueueLock = new object();

	private readonly long[] _kindCounts = new long[20];

	private readonly List<(uint From, uint To, ushort Id)> _activeTethers = new List<(uint, uint, ushort)>();

	private readonly Dictionary<uint, uint> _activeHeadmarkers = new Dictionary<uint, uint>();

	private readonly List<PullInfo> _pulls = new List<PullInfo>();

	private int _currentPull;

	private bool _inCombat;

	private DateTime _combatLeftAt = DateTime.MinValue;

	private const double SnapshotInterval = 0.125;

	private const int MaxFrames = 60000;

	private const int MaxPersistFrames = 24000;

	private readonly List<MapFrame> _frames = new List<MapFrame>();

	private readonly Dictionary<uint, string> _frameNames = new Dictionary<uint, string>();

	private readonly Dictionary<uint, string> _jobNameCache = new Dictionary<uint, string>();

	private readonly List<ActorSample> _frameScratch = new List<ActorSample>();

	private DateTime _lastSnapshot = DateTime.MinValue;

	private Hook<ActorCastDelegate>? _castHook;

	private const string CastSig = "40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1";

	private Hook<ProcessActionEffectDelegate>? _actionEffectHook;

	private Hook<ProcessActorControlDelegate>? _actorControlHook;

	private Hook<ProcessMapEffectDelegate>? _mapEffectHook;

	private const string MapEffectSig = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8";

	private Hook<ProcessPlayActionTimelineSyncDelegate>? _timelineSyncHook;

	private const string TimelineSyncSig = "48 8B D1 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53 56";

	private Hook<ProcessNpcYellDelegate>? _npcYellHook;

	private const string NpcYellSig = "48 83 EC 68 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 0F 10 41 10";

	private Hook<ActorVfxCreateDelegate>? _actorVfxHook;

	private static readonly bool InstallActorVfxHook = false;

	private const string ActorVfxSig = "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";

	private readonly List<string> _recentMapFx = new List<string>();

	private const uint CategoryTargetIcon = 34u;

	private const uint CategoryActorTargetVfx = 184u;

	private const uint CategoryPlayActionTimeline = 407u;

	private const uint CategoryEventObjectAnim = 413u;

	private const uint LogFileMagic = 1498434631u;

	private const int LogFileVersion = 4;

	private string? _logPathCache;

	public long TotalEmitted { get; private set; }

	public DateTime LastEventAt { get; private set; } = DateTime.MinValue;

	public IReadOnlyList<LiveTether> ActiveTethers
	{
		get
		{
			List<LiveTether> list = new List<LiveTether>(_activeTethers.Count);
			foreach (var activeTether in _activeTethers)
			{
				list.Add(new LiveTether(activeTether.From, activeTether.To, activeTether.Id));
			}
			return list;
		}
	}

	public IReadOnlyList<LiveHeadmarker> ActiveHeadmarkers
	{
		get
		{
			List<LiveHeadmarker> list = new List<LiveHeadmarker>(_activeHeadmarkers.Count);
			foreach (KeyValuePair<uint, uint> activeHeadmarker in _activeHeadmarkers)
			{
				list.Add(new LiveHeadmarker(activeHeadmarker.Key, activeHeadmarker.Value));
			}
			return list;
		}
	}

	public int ActorsTracked => _actors.Count;

	public IReadOnlyList<PullInfo> Pulls => _pulls;

	public IReadOnlyList<MapFrame> Frames => _frames;

	public bool ActionEffectInstalled { get; private set; }

	public bool CastHookInstalled { get; private set; }

	public string InstallError { get; private set; } = "";

	public bool MapEffectInstalled { get; private set; }

	public string MapEffectError { get; private set; } = "";

	public bool TimelineSyncInstalled { get; private set; }

	public bool NpcYellInstalled { get; private set; }

	public bool ActorVfxInstalled { get; private set; }

	public string ActorVfxError { get; private set; } = "";

	public string TimelineSyncError { get; private set; } = "";

	public string RecentMapEffects { get; private set; } = "";

	public bool ActorControlInstalled { get; private set; }

	public IReadOnlyList<LogEvent> Events => _events;

	public Func<List<MapAoe>>? ActiveAoeProvider { get; set; }

	private string LogFilePath => _logPathCache ?? (_logPathCache = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "session-log.bin"));

	public event Action<LogEvent>? OnEvent;

	public event Action<ulong, ushort>? OnNpcYell;

	private static bool HasCjk(string s)
	{
		foreach (char c in s)
		{
			if (c >= '豈')
			{
				if (c >= '\uff00')
				{
					if (c <= '\uffef')
					{
						goto IL_0066;
					}
				}
				else if (c <= '\ufaff')
				{
					goto IL_0066;
				}
			}
			else if (c >= '\u3000')
			{
				if (c >= '가')
				{
					if (c <= '\ud7af')
					{
						goto IL_0066;
					}
				}
				else if (c <= '鿿')
				{
					goto IL_0066;
				}
			}
			else if (c >= 'ᄀ' && c <= 'ᇿ')
			{
				goto IL_0066;
			}
			bool flag = false;
			goto IL_006c;
			IL_0066:
			flag = true;
			goto IL_006c;
			IL_006c:
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private static string CleanName(string? name, uint id)
	{
		if (string.IsNullOrEmpty(name) || HasCjk(name))
		{
			return $"#{id}";
		}
		return name;
	}

	public long KindCount(LogKind k)
	{
		return _kindCounts[(uint)k];
	}

	public string FrameActorName(uint id)
	{
		if (!_frameNames.TryGetValue(id, out string value))
		{
			return "";
		}
		return value;
	}

	public unsafe CombatLogCapture(Configuration config, IGameInteropProvider interop, IPluginLog log)
	{
		_config = config;
		_log = log;
		try
		{
			_actionEffectHook = interop.HookFromSignature<ProcessActionEffectDelegate>(ActionEffectHandler.Addresses.Receive.String, ActionEffectDetour);
			_actionEffectHook.Enable();
			ActionEffectInstalled = true;
		}
		catch (Exception ex)
		{
			InstallError = ex.Message;
			_log.Error(ex, "[Replica] failed to install ActionEffect hook");
		}
		try
		{
			_castHook = interop.HookFromSignature<ActorCastDelegate>("40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1", ActorCastDetour);
			_castHook.Enable();
			CastHookInstalled = true;
		}
		catch (Exception exception)
		{
			_log.Error(exception, "[Replica] failed to install ActorCast hook");
		}
		try
		{
			_actorControlHook = interop.HookFromSignature<ProcessActorControlDelegate>("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", ActorControlDetour);
			_actorControlHook.Enable();
			ActorControlInstalled = true;
		}
		catch (Exception exception2)
		{
			_log.Error(exception2, "[Replica] failed to install ActorControl hook");
		}
		try
		{
			_mapEffectHook = interop.HookFromSignature<ProcessMapEffectDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8", ProcessMapEffectDetour);
			_mapEffectHook.Enable();
			MapEffectInstalled = true;
		}
		catch (Exception ex2)
		{
			MapEffectError = ex2.Message;
			_log.Information("[Replica] MapEffect feed unavailable on this game build: " + ex2.Message);
		}
		try
		{
			_timelineSyncHook = interop.HookFromSignature<ProcessPlayActionTimelineSyncDelegate>("48 8B D1 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53 56", ProcessTimelineSyncDetour);
			_timelineSyncHook.Enable();
			TimelineSyncInstalled = true;
		}
		catch (Exception ex3)
		{
			TimelineSyncError = ex3.Message;
			_log.Information("[Replica] TimelineSync feed unavailable on this game build: " + ex3.Message);
		}
		try
		{
			_npcYellHook = interop.HookFromSignature<ProcessNpcYellDelegate>("48 83 EC 68 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 0F 10 41 10", NpcYellDetour);
			_npcYellHook.Enable();
			NpcYellInstalled = true;
		}
		catch (Exception ex4)
		{
			_log.Information("[Replica] NpcYell feed unavailable on this game build: " + ex4.Message);
		}
		VfxContainerHooks.Init(this, interop, Plugin.SigScanner, _log);
		if (InstallActorVfxHook)
		{
			try
			{
				nint procAddress = Plugin.SigScanner.ScanText("40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8");
				_actorVfxHook = interop.HookFromAddress<ActorVfxCreateDelegate>(procAddress, ActorVfxDetour);
				_actorVfxHook.Enable();
				ActorVfxInstalled = true;
			}
			catch (Exception ex5)
			{
				ActorVfxError = ex5.Message;
				_log.Information("[Replica] VFX feed unavailable on this game build: " + ex5.Message);
			}
		}
		LoadFromDisk();
		UpdateHookStates();
	}

	private nint ActorVfxDetour(nint path, nint caster, nint target, float a4, char a5, ushort a6, char a7)
	{
		nint result = _actorVfxHook.Original(path, caster, target, a4, a5, a6, a7);
		try
		{
			if (_config.LogGameVfx && ShouldCapture() && path != 0)
			{
				string text = Marshal.PtrToStringUTF8(path) ?? "";
				if (text.Length > 0)
				{
					IGameObject gameObject = AddressToObject(caster);
					IGameObject gameObject2 = AddressToObject(target);
					QueueFromHook(new LogEvent
					{
						Kind = LogKind.Vfx,
						Name = text,
						SourceId = (gameObject?.EntityId ?? 0),
						SourceName = (gameObject?.Name.TextValue ?? ""),
						SourceKind = ((gameObject is IBattleChara bc) ? Classify(bc) : ActorKind.Other),
						TargetId = (gameObject2?.EntityId ?? 0),
						TargetName = (gameObject2?.Name.TextValue ?? ""),
						TargetKind = ((gameObject2 is IBattleChara bc2) ? Classify(bc2) : ActorKind.Other)
					}, _config.ShowVfx);
				}
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] vfx detour: " + ex.Message);
		}
		return result;
	}

	private static IGameObject? AddressToObject(nint addr)
	{
		if (addr == 0)
		{
			return null;
		}
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (item.Address == addr)
			{
				return item;
			}
		}
		return null;
	}

	private unsafe void NpcYellDetour(NpcYellPacket* data)
	{
		_npcYellHook.Original(data);
		try
		{
			if (!ShouldCapture() || data == null)
			{
				return;
			}
			try
			{
				OnNpcYell?.Invoke(data->SourceId, data->MessageId);
			}
			catch (Exception ex)
			{
				_log.Debug("[Replica] npc-yell dispatch: " + ex.Message);
			}
		}
		catch (Exception ex2)
		{
			_log.Debug("[Replica] npc-yell error: " + ex2.Message);
		}
	}

	private unsafe void ProcessTimelineSyncDetour(PlayActionTimelineSyncPacket* data)
	{
		_timelineSyncHook.Original(data);
		try
		{
			if (!ShouldCapture() || data == null)
			{
				return;
			}
			uint num = 0u;
			for (int i = 0; i < 10; i++)
			{
				uint num2 = data->EntityIds[i];
				if (num2 != 3758096384u)
				{
					if (num == 0)
					{
						num = num2;
					}
					IGameObject gameObject = Plugin.ObjectTable.SearchById(num);
					QueueFromHook(new LogEvent
					{
						Kind = LogKind.TimelineSync,
						SourceId = num,
						SourceName = (gameObject?.Name.TextValue ?? ""),
						SourceKind = ((gameObject is IBattleChara bc) ? Classify(bc) : ActorKind.Other),
						TargetId = num2,
						DataId = data->TimelineIds[i],
						Name = $"TimelineSync {data->TimelineIds[i]:X4}"
					}, _config.ShowControl);
					continue;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] timeline-sync error: " + ex.Message);
		}
	}

	private static void Toggle<T>(Hook<T>? h, bool enabled) where T : Delegate
	{
		if (h == null)
		{
			return;
		}
		try
		{
			if (enabled)
			{
				h.Enable();
			}
			else
			{
				h.Disable();
			}
		}
		catch
		{
		}
	}

	public void SetAllGameHooks(bool enabled)
	{
		Toggle(_actionEffectHook, enabled);
		Toggle(_castHook, enabled);
		Toggle(_actorControlHook, enabled);
		Toggle(_mapEffectHook, enabled);
		Toggle(_timelineSyncHook, enabled);
		Toggle(_npcYellHook, enabled);
		Toggle(_actorVfxHook, enabled);
		VfxContainerHooks.SetEnabled(enabled);
	}

	public bool SetGameHook(string name, bool enabled)
	{
		switch (name)
		{
		case "actioneffect":
			Toggle(_actionEffectHook, enabled);
			return true;
		case "cast":
			Toggle(_castHook, enabled);
			return true;
		case "actorcontrol":
			Toggle(_actorControlHook, enabled);
			return true;
		case "mapeffect":
			Toggle(_mapEffectHook, enabled);
			return true;
		case "timelinesync":
			Toggle(_timelineSyncHook, enabled);
			return true;
		case "npcyell":
			Toggle(_npcYellHook, enabled);
			return true;
		case "vfx":
			Toggle(_actorVfxHook, enabled);
			return true;
		case "tether":
			VfxContainerHooks.SetEnabled(enabled);
			return true;
		default:
			return false;
		}
	}

	public void Dispose()
	{
		SaveToDisk();
		try
		{
			_actionEffectHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_castHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_actorControlHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_mapEffectHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_timelineSyncHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_npcYellHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_actorVfxHook?.Dispose();
		}
		catch
		{
		}
		VfxContainerHooks.Dispose();
	}

	private unsafe void ProcessMapEffectDetour(void* self, uint index, ushort s1, ushort s2)
	{
		_mapEffectHook.Original(self, index, s1, s2);
		try
		{
			if (!ShouldCapture())
			{
				return;
			}
			uint num = (uint)(s1 | (s2 << 16));
			string text = $"{num:X8}@{index:X2}";
			if (_recentMapFx.Count != 0)
			{
				List<string> recentMapFx = _recentMapFx;
				if (!(recentMapFx[recentMapFx.Count - 1] != text))
				{
					goto IL_00c4;
				}
			}
			_recentMapFx.Add(text);
			if (_recentMapFx.Count > 8)
			{
				_recentMapFx.RemoveAt(0);
			}
			RecentMapEffects = string.Join(" ", _recentMapFx);
			goto IL_00c4;
			IL_00c4:
			QueueFromHook(new LogEvent
			{
				Kind = LogKind.MapEffect,
				SourceKind = ActorKind.Other,
				Name = "MapEffect",
				Category = num,
				Param1 = index
			}, _config.ShowMapFx);
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] map-effect error: " + ex.Message);
		}
	}

	public void UpdateHookStates()
	{
		bool active = _config.CaptureWhen != CaptureMode.Disabled;
		SetAllGameHooks(active);
	}

	public void TrimPulls()
	{
		if (_config.CaptureWhen == CaptureMode.Disabled)
		{
			Clear();
			return;
		}
		if (!_config.LogActions)
		{
			_events.RemoveAll(e => e.IsCast);
		}
		int maxPulls = _config.MaxPullsToKeep;
		if (maxPulls <= 0)
		{
			return;
		}
		while (_pulls.Count > maxPulls)
		{
			int oldestIndex = _pulls[0].Index;
			_pulls.RemoveAt(0);
			_events.RemoveAll(e => e.Pull == oldestIndex);
			_frames.RemoveAll(f => f.Pull == oldestIndex);
		}

		// Trim out-of-combat events (Pull == 0)
		if (_pulls.Count > 0)
		{
			DateTime oldestPullStart = _pulls[0].Start;
			_events.RemoveAll(e => e.Pull == 0 && e.Time < oldestPullStart);
		}
		else
		{
			// If no pulls, cap Pull == 0 events at 5000 in memory
			if (_events.Count > 5000)
			{
				_events.RemoveRange(0, _events.Count - 5000);
			}
		}
	}

	public void Clear()
	{
		_events.Clear();
		_pulls.Clear();
		_frames.Clear();
		_frameNames.Clear();
		_jobNameCache.Clear();
		_currentPull = 0;
		ResetLiveState();
		try
		{
			if (File.Exists(LogFilePath))
			{
				File.Delete(LogFilePath);
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] log wipe failed: " + ex.Message);
		}
	}

	public void ResetLiveState()
	{
		_activeTethers.Clear();
		_activeHeadmarkers.Clear();
		Data.TetherPlayer.Clear();
	}

	public static string SanitizeForFileName(string? input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return "Unknown";
		}
		char[] invalid = Path.GetInvalidFileNameChars();
		StringBuilder sb = new StringBuilder(input.Length);
		foreach (char c in input)
		{
			if (invalid.Contains(c) || c == ':' || c == '/' || c == '\\' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|')
			{
				sb.Append('-');
			}
			else if (c == ' ')
			{
				sb.Append('_');
			}
			else
			{
				sb.Append(c);
			}
		}
		string res = sb.ToString().Trim('-', '_');
		while (res.Contains("__"))
		{
			res = res.Replace("__", "_");
		}
		while (res.Contains("--"))
		{
			res = res.Replace("--", "-");
		}
		return string.IsNullOrEmpty(res) ? "Unknown" : res;
	}

	public static string DetectCurrentBossName()
	{
		try
		{
			// 1. BossMod active module
			string? bmBoss = Plugin.Instance?.BossModBridge?.GetActiveBossName();
			if (!string.IsNullOrWhiteSpace(bmBoss) && bmBoss != "none" && bmBoss != "(none)")
			{
				return bmBoss;
			}

			// 2. Replica custom Host FightName
			var host = Plugin.Instance?.Host;
			if (host != null)
			{
				string fName = host.FightName;
				if (!string.IsNullOrWhiteSpace(fName) && fName != "none" && fName != "(none)")
				{
					return fName;
				}
			}

			// 3. Current local player target if enemy
			if (Plugin.ObjectTable.LocalPlayer?.TargetObject is IBattleChara target && Classify(target) == ActorKind.Enemy)
			{
				string tName = target.Name.TextValue;
				if (!string.IsNullOrWhiteSpace(tName))
				{
					return tName;
				}
			}

			// 4. Scan ObjectTable for highest MaxHP enemy
			IBattleChara? bestEnemy = null;
			uint maxHp = 0;
			foreach (var obj in Plugin.ObjectTable)
			{
				if (obj is IBattleChara bc && Classify(bc) == ActorKind.Enemy)
				{
					if (bc.MaxHp > maxHp && !string.IsNullOrWhiteSpace(bc.Name.TextValue))
					{
						maxHp = bc.MaxHp;
						bestEnemy = bc;
					}
				}
			}
			if (bestEnemy != null && !string.IsNullOrWhiteSpace(bestEnemy.Name.TextValue))
			{
				return bestEnemy.Name.TextValue;
			}
		}
		catch
		{
		}
		return "";
	}

	public void SaveToDisk()
	{
		try
		{
			Directory.CreateDirectory(Plugin.PluginInterface.GetPluginConfigDirectory());
			using FileStream output = new FileStream(LogFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.UTF8);
			binaryWriter.Write(1498434631u);
			binaryWriter.Write(7);
			binaryWriter.Write(_seq);
			binaryWriter.Write(_pulls.Count);
			foreach (PullInfo pull in _pulls)
			{
				binaryWriter.Write(pull.Index);
				binaryWriter.Write(pull.Label);
				binaryWriter.Write(pull.Start.Ticks);
				binaryWriter.Write(pull.End.Ticks);
				binaryWriter.Write(pull.Events);
				binaryWriter.Write(pull.Territory);
				binaryWriter.Write(pull.MapId);
				binaryWriter.Write(pull.ZoneName ?? "");
				binaryWriter.Write(pull.BossName ?? "");
			}
			binaryWriter.Write(_events.Count);
			foreach (LogEvent @event in _events)
			{
				WriteEvent(binaryWriter, @event);
			}
			WriteFrames(binaryWriter);
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] log save failed: " + ex.Message);
		}
	}

	private void WriteFrames(BinaryWriter w)
	{
		int num = Math.Max(0, _frames.Count - 24000);
		w.Write(_frames.Count - num);
		for (int i = num; i < _frames.Count; i++)
		{
			MapFrame mapFrame = _frames[i];
			w.Write(mapFrame.Pull);
			w.Write(mapFrame.T);
			w.Write(mapFrame.Actors.Length);
			ActorSample[] actors = mapFrame.Actors;
			for (int j = 0; j < actors.Length; j++)
			{
				ActorSample actorSample = actors[j];
				w.Write(actorSample.Id);
				w.Write((byte)actorSample.Kind);
				w.Write(actorSample.X);
				w.Write(actorSample.Z);
				w.Write(actorSample.Rot);
				w.Write(actorSample.HpPct);
				w.Write(actorSample.CastId);
			}
			int aoeCount = mapFrame.Aoes != null ? mapFrame.Aoes.Length : 0;
			w.Write(aoeCount);
			if (mapFrame.Aoes != null)
			{
				for (int k = 0; k < aoeCount; k++)
				{
					ref readonly var aoe = ref mapFrame.Aoes[k];
					w.Write((byte)aoe.Kind);
					w.Write(aoe.IsSafe);
					w.Write(aoe.Flags);
					w.Write(aoe.X);
					w.Write(aoe.Z);
					w.Write(aoe.Rot);
					w.Write(aoe.Param1);
					w.Write(aoe.Param2);
					w.Write(aoe.Param3);
					w.Write(aoe.Color);
					w.Write(aoe.SourceId);
					w.Write(aoe.ActionId);
					w.Write(aoe.TargetId);
				}
			}
		}
		w.Write(_frameNames.Count);
		foreach (KeyValuePair<uint, string> frameName in _frameNames)
		{
			w.Write(frameName.Key);
			w.Write(frameName.Value);
		}
	}

	private void LoadFromDisk()
	{
		try
		{
			if (!File.Exists(LogFilePath))
			{
				return;
			}
			using FileStream input = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using BinaryReader binaryReader = new BinaryReader(input, Encoding.UTF8);
			if (binaryReader.ReadUInt32() != 1498434631)
			{
				return;
			}
			int num = binaryReader.ReadInt32();
			if (num < 1 || num > 6)
			{
				return;
			}
			_seq = binaryReader.ReadInt64();
			int num2 = binaryReader.ReadInt32();
			_pulls.Clear();
			for (int i = 0; i < num2; i++)
			{
				PullInfo pullInfo = new PullInfo
				{
					Index = binaryReader.ReadInt32(),
					Label = binaryReader.ReadString(),
					Start = new DateTime(binaryReader.ReadInt64()),
					End = new DateTime(binaryReader.ReadInt64()),
					Events = binaryReader.ReadInt32()
				};
				if (num >= 3)
				{
					pullInfo.Territory = binaryReader.ReadUInt32();
				}
				if (num >= 4)
				{
					pullInfo.MapId = binaryReader.ReadUInt32();
				}
				if (num >= 6)
				{
					pullInfo.ZoneName = binaryReader.ReadString();
					pullInfo.BossName = binaryReader.ReadString();
				}
				if (string.IsNullOrEmpty(pullInfo.Label) || pullInfo.Label.StartsWith("Pull "))
				{
					pullInfo.Label = pullInfo.GetFullDisplayLabel();
				}
				_pulls.Add(pullInfo);
			}
			int num3 = binaryReader.ReadInt32();
			_events.Clear();
			_events.Capacity = Math.Max(_events.Capacity, num3);
			for (int j = 0; j < num3; j++)
			{
				_events.Add(ReadEvent(binaryReader));
			}
			if (num >= 2)
			{
				ReadFrames(binaryReader, num);
			}
			_currentPull = 0;
			TrimPulls();
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] log load failed: " + ex.Message);
		}
	}

	private void ReadFrames(BinaryReader r, int version)
	{
		int num = r.ReadInt32();
		_frames.Clear();
		_frames.Capacity = Math.Max(_frames.Capacity, num);
		for (int i = 0; i < num; i++)
		{
			int pull = r.ReadInt32();
			double t = r.ReadDouble();
			int num2 = r.ReadInt32();
			ActorSample[] array = new ActorSample[num2];
			for (int j = 0; j < num2; j++)
			{
				array[j] = new ActorSample(r.ReadUInt32(), (ActorKind)r.ReadByte(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadUInt32());
			}
			MapAoe[] aoes = Array.Empty<MapAoe>();
			if (version >= 5)
			{
				int numAoes = r.ReadInt32();
				if (numAoes > 0)
				{
					aoes = new MapAoe[numAoes];
					for (int k = 0; k < numAoes; k++)
					{
						MapAoeKind kind = (MapAoeKind)r.ReadByte();
						bool isSafe = r.ReadBoolean();
						ushort flags = r.ReadUInt16();
						float x = r.ReadSingle();
						float z = r.ReadSingle();
						float rot = r.ReadSingle();
						float p1 = r.ReadSingle();
						float p2 = r.ReadSingle();
						float p3 = r.ReadSingle();
						uint color = r.ReadUInt32();
						uint sourceId = (version >= 7) ? r.ReadUInt32() : 0u;
						uint actionId = (version >= 7) ? r.ReadUInt32() : 0u;
						uint targetId = (version >= 7) ? r.ReadUInt32() : 0u;
						aoes[k] = new MapAoe(kind, isSafe, x, z, rot, p1, p2, p3, color, flags, sourceId, actionId, targetId);
					}
				}
			}
			_frames.Add(new MapFrame
			{
				Pull = pull,
				T = t,
				Actors = array,
				Aoes = aoes
			});
		}
		int num3 = r.ReadInt32();
		_frameNames.Clear();
		for (int k = 0; k < num3; k++)
		{
			uint key = r.ReadUInt32();
			_frameNames[key] = r.ReadString();
		}
	}

	private static void WriteEvent(BinaryWriter w, LogEvent e)
	{
		w.Write(e.Seq);
		w.Write(e.Time.Ticks);
		w.Write((byte)e.Kind);
		w.Write(e.Pull);
		w.Write(e.SourceName);
		w.Write(e.SourceId);
		w.Write((byte)e.SourceKind);
		w.Write(e.TargetName);
		w.Write(e.TargetId);
		w.Write((byte)e.TargetKind);
		w.Write(e.Name);
		w.Write(e.DataId);
		w.Write(e.IconId);
		w.Write(e.Value);
		w.Write(e.Count);
		w.Write(e.X);
		w.Write(e.Y);
		w.Write(e.Heading);
		w.Write(e.Category);
		w.Write(e.Param1);
		w.Write(e.Param2);
		w.Write(e.Param3);
		w.Write(e.Param4);
	}

	private static LogEvent ReadEvent(BinaryReader r)
	{
		return new LogEvent
		{
			Seq = r.ReadInt64(),
			Time = new DateTime(r.ReadInt64()),
			Kind = (LogKind)r.ReadByte(),
			Pull = r.ReadInt32(),
			SourceName = r.ReadString(),
			SourceId = r.ReadUInt32(),
			SourceKind = (ActorKind)r.ReadByte(),
			TargetName = r.ReadString(),
			TargetId = r.ReadUInt32(),
			TargetKind = (ActorKind)r.ReadByte(),
			Name = r.ReadString(),
			DataId = r.ReadUInt32(),
			IconId = r.ReadUInt32(),
			Value = r.ReadSingle(),
			Count = r.ReadUInt32(),
			X = r.ReadSingle(),
			Y = r.ReadSingle(),
			Heading = r.ReadSingle(),
			Category = r.ReadUInt32(),
			Param1 = r.ReadUInt32(),
			Param2 = r.ReadUInt32(),
			Param3 = r.ReadUInt32(),
			Param4 = r.ReadUInt32()
		};
	}

	private unsafe static uint CurrentMapId()
	{
		AgentMap* ptr = AgentMap.Instance();
		if (ptr == null)
		{
			return 0u;
		}
		return ptr->CurrentMapId;
	}

	public void NotifyCombat(bool inCombat)
	{
		if (inCombat == _inCombat)
		{
			return;
		}
		_inCombat = inCombat;
		if (inCombat)
		{
			if (_currentPull == 0 || (DateTime.Now - _combatLeftAt).TotalSeconds > 8.0)
			{
				ResetLiveState();
				int lastIndex = _pulls.Count > 0 ? _pulls[_pulls.Count - 1].Index : 0;
				_currentPull = lastIndex + 1;
				uint terr = Plugin.ClientState.TerritoryType;
				uint mapId = CurrentMapId();
				string zone = ZoneLibrary.NameOf(terr);
				string boss = DetectCurrentBossName();
				var pullInfo = new PullInfo
				{
					Index = _currentPull,
					Start = DateTime.Now,
					End = DateTime.MinValue,
					Territory = terr,
					MapId = mapId,
					ZoneName = zone,
					BossName = boss
				};
				pullInfo.Label = pullInfo.GetFullDisplayLabel();
				_pulls.Add(pullInfo);
				TrimPulls();
			}
		}
		else
		{
			_combatLeftAt = DateTime.Now;
			PullInfo pullInfo = _pulls.Find((PullInfo x) => x.Index == _currentPull);
			if (pullInfo != null)
			{
				if (string.IsNullOrEmpty(pullInfo.BossName))
				{
					string detected = DetectCurrentBossName();
					if (!string.IsNullOrEmpty(detected))
					{
						pullInfo.BossName = detected;
					}
				}
				pullInfo.End = DateTime.Now;
				pullInfo.Label = pullInfo.GetFullDisplayLabel();
			}
			SaveToDisk();
		}
	}

	public void Ingest(LogEvent e, bool addToLog = true)
	{
		Emit(e, addToLog);
	}

	private void QueueFromHook(LogEvent e, bool addToLog = true)
	{
		if (addToLog && !_config.LogActions && e.IsCast)
		{
			addToLog = false;
		}
		lock (_hookQueueLock)
		{
			_hookQueue.Enqueue((e, addToLog));
		}
	}

	private void DrainHookQueue()
	{
		while (true)
		{
			(LogEvent, bool) tuple;
			lock (_hookQueueLock)
			{
				if (_hookQueue.Count == 0)
				{
					break;
				}
				tuple = _hookQueue.Dequeue();
			}
			Emit(tuple.Item1, tuple.Item2);
		}
	}

	public void Update()
	{
		DrainHookQueue();
		try
		{
			if (!ShouldCapture())
			{
				return;
			}
			foreach (ActorState value in _actors.Values)
			{
				value.SeenThisPass = false;
			}
			_eventObjScratch.Clear();
			foreach (IGameObject item in Plugin.ObjectTable)
			{
				if (item.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.EventObj)
				{
					uint entityId = item.EntityId;
					_eventObjScratch.Add(entityId);
					if (!_eventObjs.Contains(entityId))
					{
						EmitAddedEventObj(item);
					}
				}
				else if (item is IBattleChara bc)
				{
					ActorKind actorKind = Classify(bc);
					if (actorKind != ActorKind.Other)
					{
						Poll(bc, actorKind);
					}
				}
			}
			_eventObjs.Clear();
			foreach (uint item2 in _eventObjScratch)
			{
				_eventObjs.Add(item2);
			}
			foreach (uint item3 in (from kv in _actors
				where !kv.Value.SeenThisPass
				select kv.Key).ToList())
			{
				_actors.Remove(item3);
			}
			SampleFrame();
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] capture error: " + ex.Message);
		}
	}

	private void SampleFrame()
	{
		if (_currentPull == 0 || !_inCombat)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if ((now - _lastSnapshot).TotalSeconds < 0.125)
		{
			return;
		}
		_lastSnapshot = now;
		PullInfo pullInfo = _pulls.Find((PullInfo x) => x.Index == _currentPull);
		if (pullInfo == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(pullInfo.BossName) || pullInfo.BossName == "Unknown")
		{
			string detected = DetectCurrentBossName();
			if (!string.IsNullOrEmpty(detected))
			{
				pullInfo.BossName = detected;
				pullInfo.Label = pullInfo.GetFullDisplayLabel();
			}
		}
		_frameScratch.Clear();
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (!(item is IBattleChara battleChara))
			{
				continue;
			}
			ActorKind actorKind = Classify(battleChara);
			if (actorKind == ActorKind.Other)
			{
				continue;
			}
			float hpPct = ((battleChara.MaxHp != 0) ? ((float)battleChara.CurrentHp / (float)battleChara.MaxHp * 100f) : (-1f));
			uint castId = (battleChara.IsCasting ? battleChara.CastActionId : 0u);
			_frameScratch.Add(new ActorSample(battleChara.EntityId, actorKind, battleChara.Position.X, battleChara.Position.Z, battleChara.Rotation, hpPct, castId));
			if (!_frameNames.ContainsKey(battleChara.EntityId))
			{
				string value = ((battleChara is IPlayerCharacter pc) ? JobAbbr(pc) : battleChara.Name.TextValue);
				if (!string.IsNullOrEmpty(value))
				{
					_frameNames[battleChara.EntityId] = value;
				}
			}
		}
		if (_frameScratch.Count != 0)
		{
			MapAoe[]? frameAoes = null;
			if (ActiveAoeProvider != null)
			{
				try
				{
					List<MapAoe>? list = ActiveAoeProvider();
					if (list != null && list.Count > 0)
					{
						frameAoes = list.ToArray();
					}
				}
				catch
				{
				}
			}
			_frames.Add(new MapFrame
			{
				Pull = _currentPull,
				T = (now - pullInfo.Start).TotalSeconds,
				Actors = _frameScratch.ToArray(),
				Aoes = frameAoes ?? Array.Empty<MapAoe>()
			});
			if (_frames.Count > 60000)
			{
				_frames.RemoveRange(0, _frames.Count - 60000);
			}
		}
	}

	public bool ShouldCapture()
	{
		if (_config.CaptureWhen == CaptureMode.Disabled)
		{
			return false;
		}
		bool inActiveCapture = _config.CaptureWhen switch
		{
			CaptureMode.InCombat => Plugin.Condition[ConditionFlag.InCombat] || Plugin.DutyState.IsDutyStarted, 
			CaptureMode.InDuty => Plugin.DutyState.IsDutyStarted, 
			_ => true, 
		};
		if (!inActiveCapture)
		{
			return false;
		}
		// Discard out-of-combat events (where _currentPull == 0) if LogOutsidePulls is disabled
		if (_currentPull == 0 && !_config.LogOutsidePulls)
		{
			return false;
		}
		return true;
	}

	private void Poll(IBattleChara bc, ActorKind kind)
	{
		if (!_actors.TryGetValue(bc.EntityId, out ActorState value))
		{
			value = new ActorState
			{
				Alive = (!bc.IsDead && bc.CurrentHp != 0)
			};
			foreach (IStatus status in bc.StatusList)
			{
				if (status.StatusId != 0)
				{
					value.Statuses.Add((status.StatusId, status.SourceId));
				}
			}
			_actors[bc.EntityId] = value;
			if (kind == ActorKind.Enemy)
			{
				EmitAdded(bc, kind);
			}
			EmitSpawnTether(bc);
		}
		value.SeenThisPass = true;
		PollCast(bc, kind, value);
		PollStatuses(bc, kind, value);
		PollDeath(bc, kind, value);
	}

	private unsafe void EmitSpawnTether(IBattleChara bc)
	{
		Character* address = (Character*)bc.Address;
		if (address != null)
		{
			VfxContainer.Tether tether = address->Vfx.Tethers[0];
			if (tether.Id != 0)
			{
				NotifyTetherFromVfx(bc.EntityId, (uint)(ulong)tether.TargetId, tether.Id);
			}
		}
	}

	internal void NotifyTetherFromVfx(uint from, uint to, ushort tetherId)
	{
		if (ShouldCapture() && to != 3758096384u)
		{
			int num = _activeTethers.FindIndex(((uint From, uint To, ushort Id) t) => t.From == from && t.To == to);
			if (num >= 0)
			{
				_activeTethers[num] = (from, to, tetherId);
				UpdateTetherStore(from, to, tetherId);
			}
			else
			{
				_activeTethers.Add((from, to, tetherId));
				UpdateTetherStore(from, to, tetherId);
				QueueTether(from, to, tetherId);
			}
		}
	}

	private static void UpdateTetherStore(uint from, uint to, ushort tetherId)
	{
		TetherInfo tetherInfo = Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == from && t.To == to);
		if (tetherInfo != null)
		{
			tetherInfo.TetherID = tetherId;
		}
		else
		{
			Data.TetherPlayer.Add(new TetherInfo(from, to, tetherId));
		}
	}

	internal void NotifyTetherCancelFromVfx(uint from)
	{
		if (ShouldCapture())
		{
			_activeTethers.RemoveAll(((uint From, uint To, ushort Id) t) => t.From == from);
			Data.TetherPlayer.RemoveAll((TetherInfo t) => t.From == from);
			QueueFromHook(new LogEvent
			{
				Kind = LogKind.TetherCancel,
				SourceId = from,
				Name = "Tether Cancel"
			});
		}
	}

	private void QueueTether(uint from, uint to, ushort tetherId)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(from);
		IGameObject gameObject2 = ((to != 0) ? Plugin.ObjectTable.SearchById(to) : null);
		ActorKind sourceKind = ((gameObject is IBattleChara bc) ? Classify(bc) : ActorKind.Other);
		QueueFromHook(new LogEvent
		{
			Kind = LogKind.Tether,
			SourceId = from,
			SourceName = (gameObject?.Name.TextValue ?? ""),
			SourceKind = sourceKind,
			TargetId = to,
			TargetName = (gameObject2?.Name.TextValue ?? ""),
			TargetKind = ((gameObject2 is IBattleChara bc2) ? Classify(bc2) : ActorKind.Other),
			DataId = tetherId,
			Name = $"Tether {tetherId:X4}"
		});
	}

	private void PollCast(IBattleChara bc, ActorKind kind, ActorState state)
	{
		uint num = (bc.IsCasting ? bc.CastActionId : 0u);
		uint lastCastId = state.LastCastId;
		if (num == lastCastId)
		{
			return;
		}
		state.LastCastId = num;
		if (lastCastId != 0)
		{
			Lumina.Excel.Sheets.Action? rowOrDefault = Plugin.Actions.GetRowOrDefault(lastCastId);
			System.Numerics.Vector3 position = bc.Position;
			Emit(new LogEvent
			{
				Kind = LogKind.CastFinish,
				SourceName = bc.Name.TextValue,
				SourceId = bc.EntityId,
				SourceKind = kind,
				Name = CleanName(rowOrDefault?.Name.ExtractText(), lastCastId),
				DataId = lastCastId,
				IconId = (rowOrDefault?.Icon ?? 0),
				X = position.X,
				Y = position.Z,
				Heading = bc.Rotation
			});
		}
		if (num == 0 || (CastHookInstalled && kind == ActorKind.Enemy))
		{
			return;
		}
		Lumina.Excel.Sheets.Action? rowOrDefault2 = Plugin.Actions.GetRowOrDefault(num);
		string targetName = "";
		if (bc.CastTargetObjectId != 0L)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById((uint)bc.CastTargetObjectId);
			if (gameObject != null)
			{
				targetName = gameObject.Name.TextValue;
			}
		}
		System.Numerics.Vector3 position2 = bc.Position;
		Emit(new LogEvent
		{
			Kind = LogKind.CastStart,
			SourceName = bc.Name.TextValue,
			SourceId = bc.EntityId,
			SourceKind = kind,
			TargetName = targetName,
			TargetId = (uint)bc.CastTargetObjectId,
			Name = CleanName(rowOrDefault2?.Name.ExtractText(), num),
			DataId = num,
			IconId = (rowOrDefault2?.Icon ?? 0),
			Value = bc.TotalCastTime,
			X = position2.X,
			Y = position2.Z,
			Heading = bc.Rotation
		});
	}

	private unsafe void ActorCastDetour(uint casterId, ActorCast* data)
	{
		_castHook.Original(casterId, data);
		try
		{
			if (!ShouldCapture() || data == null || data->ActionKind != 1 || !(Plugin.ObjectTable.SearchById(casterId) is IBattleChara battleChara) || Classify(battleChara) != ActorKind.Enemy)
			{
				return;
			}
			System.Numerics.Vector3 pos = data->Pos;
			Data.LastCastPositions[casterId] = pos;
			float castRotation = ((Character*)battleChara.Address)->CastRotation;
			Lumina.Excel.Sheets.Action? rowOrDefault = Plugin.Actions.GetRowOrDefault(data->ActionId);
			string targetName = "";
			if (data->TargetId != 0)
			{
				IGameObject gameObject = Plugin.ObjectTable.SearchById(data->TargetId);
				if (gameObject != null)
				{
					targetName = gameObject.Name.TextValue;
				}
			}
			QueueFromHook(new LogEvent
			{
				Kind = LogKind.CastStart,
				SourceName = battleChara.Name.TextValue,
				SourceId = casterId,
				SourceKind = ActorKind.Enemy,
				TargetName = targetName,
				TargetId = data->TargetId,
				Name = CleanName(rowOrDefault?.Name.ExtractText(), data->ActionId),
				DataId = data->ActionId,
				IconId = (rowOrDefault?.Icon ?? 0),
				Value = data->CastTime + 0.3f,
				Param1 = data->DisplayDelay,
				X = pos.X,
				Y = pos.Z,
				Heading = castRotation
			});
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] actor-cast error: " + ex.Message);
		}
	}

	private void PollStatuses(IBattleChara bc, ActorKind kind, ActorState state)
	{
		HashSet<(uint, uint)> statusScratch = _statusScratch;
		statusScratch.Clear();
		foreach (IStatus status in bc.StatusList)
		{
			if (status.StatusId != 0)
			{
				statusScratch.Add((status.StatusId, status.SourceId));
			}
		}
		foreach (var item in statusScratch)
		{
			if (!state.Statuses.Contains(item))
			{
				EmitStatus(LogKind.StatusGain, bc, kind, item);
			}
		}
		foreach (var status2 in state.Statuses)
		{
			if (!statusScratch.Contains(status2))
			{
				EmitStatus(LogKind.StatusLose, bc, kind, status2);
			}
		}
		state.Statuses.Clear();
		foreach (var item2 in statusScratch)
		{
			state.Statuses.Add(item2);
		}
	}

	private void EmitStatus(LogKind kind, IBattleChara target, ActorKind targetKind, (uint id, uint src) s)
	{
		Status? rowOrDefault = Plugin.Statuses.GetRowOrDefault(s.id);
		object obj;
		if (s.src != 0)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(s.src);
			if (gameObject != null)
			{
				obj = gameObject.Name.TextValue;
				goto IL_005d;
			}
		}
		obj = "";
		goto IL_005d;
		IL_005d:
		string sourceName = (string)obj;
		float value = 0f;
		uint count = 0u;
		if (kind == LogKind.StatusGain)
		{
			IStatus status = target.StatusList.FirstOrDefault((IStatus x) => x.StatusId == s.id);
			if (status != null)
			{
				value = status.RemainingTime;
				count = status.Param;
			}
		}
		Emit(new LogEvent
		{
			Kind = kind,
			SourceName = sourceName,
			SourceId = s.src,
			SourceKind = ActorKind.Other,
			TargetName = target.Name.TextValue,
			TargetId = target.EntityId,
			TargetKind = targetKind,
			Name = CleanName(rowOrDefault?.Name.ExtractText(), s.id),
			DataId = s.id,
			IconId = (rowOrDefault?.Icon ?? 0),
			Value = value,
			Count = count
		});
	}

	private void EmitAdded(IBattleChara bc, ActorKind kind)
	{
		System.Numerics.Vector3 position = bc.Position;
		Emit(new LogEvent
		{
			Kind = LogKind.Added,
			SourceName = bc.Name.TextValue,
			SourceId = bc.EntityId,
			SourceKind = kind,
			TargetName = bc.Name.TextValue,
			TargetId = bc.EntityId,
			Name = bc.Name.TextValue,
			DataId = bc.BaseId,
			X = position.X,
			Y = position.Z
		}, _config.ShowAdds);
	}

	private void EmitAddedEventObj(IGameObject obj)
	{
		System.Numerics.Vector3 position = obj.Position;
		Emit(new LogEvent
		{
			Kind = LogKind.Added,
			SourceName = obj.Name.TextValue,
			SourceId = obj.EntityId,
			SourceKind = ActorKind.Other,
			TargetName = obj.Name.TextValue,
			TargetId = obj.EntityId,
			Name = obj.Name.TextValue,
			DataId = obj.BaseId,
			X = position.X,
			Y = position.Z
		}, _config.ShowAdds);
	}

	private void PollDeath(IBattleChara bc, ActorKind kind, ActorState state)
	{
		bool flag = bc.IsDead || bc.CurrentHp == 0;
		if (flag && state.Alive)
		{
			Emit(new LogEvent
			{
				Kind = LogKind.Death,
				SourceName = bc.Name.TextValue,
				SourceId = bc.EntityId,
				SourceKind = kind,
				TargetName = bc.Name.TextValue,
				TargetId = bc.EntityId,
				Name = "Death"
			});
		}
		state.Alive = !flag;
	}

	public void Note(string msg)
	{
		LogEvent logEvent = new LogEvent
		{
			Seq = ++_seq,
			Time = DateTime.Now,
			Pull = _currentPull,
			Kind = LogKind.Note,
			SourceKind = ActorKind.Other,
			Name = (msg ?? "")
		};
		TotalEmitted++;
		LastEventAt = logEvent.Time;
		_kindCounts[13]++;
		_events.Add(logEvent);
		if (_events.Count > 200000)
		{
			_events.RemoveRange(0, _events.Count - 200000);
		}
		PullInfo pullInfo = _pulls.Find((PullInfo x) => x.Index == _currentPull);
		if (pullInfo != null)
		{
			pullInfo.Events++;
		}
	}

	private static string JobAbbr(IPlayerCharacter pc)
	{
		try
		{
			string text = Plugin.DataManager.GetExcelSheet<ClassJob>().GetRowOrDefault(pc.ClassJob.RowId)?.Abbreviation.ExtractText();
			return string.IsNullOrEmpty(text) ? "Player" : text;
		}
		catch
		{
			return "Player";
		}
	}

	private string AnonName(uint id, ActorKind kind, string fallback)
	{
		bool flag = id == 0;
		if (!flag)
		{
			bool flag2 = kind - 1 <= ActorKind.You;
			flag = !flag2;
		}
		if (flag)
		{
			return fallback;
		}
		if (_jobNameCache.TryGetValue(id, out string value))
		{
			return value;
		}
		if (!(Plugin.ObjectTable.SearchById(id) is IPlayerCharacter pc))
		{
			return fallback;
		}
		string text = JobAbbr(pc);
		_jobNameCache[id] = text;
		return text;
	}

	private void Emit(LogEvent e, bool addToLog = true)
	{
		if (addToLog && !_config.LogActions && e.IsCast)
		{
			addToLog = false;
		}
		string sourceName = AnonName(e.SourceId, e.SourceKind, e.SourceName);
		string targetName = AnonName(e.TargetId, e.TargetKind, e.TargetName);
		e = e with
		{
			Seq = ++_seq,
			Time = DateTime.Now,
			Pull = _currentPull,
			SourceName = sourceName,
			TargetName = targetName
		};
		TotalEmitted++;
		LastEventAt = e.Time;
		if ((int)e.Kind < _kindCounts.Length)
		{
			_kindCounts[(uint)e.Kind]++;
		}
		if (addToLog)
		{
			_events.Add(e);
			if (_events.Count > 200000)
			{
				_events.RemoveRange(0, _events.Count - 200000);
			}
			PullInfo pullInfo = _pulls.Find((PullInfo x) => x.Index == _currentPull);
			if (pullInfo != null)
			{
				pullInfo.Events++;
				if (string.IsNullOrEmpty(pullInfo.BossName) && e.SourceKind == ActorKind.Enemy && !string.IsNullOrWhiteSpace(e.SourceName))
				{
					if (e.Kind is LogKind.CastStart or LogKind.Ability)
					{
						pullInfo.BossName = e.SourceName;
						pullInfo.Label = pullInfo.GetFullDisplayLabel();
					}
				}
			}
		}
		try
		{
			OnEvent?.Invoke(e);
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] trigger error: " + ex.Message);
		}
	}

	private unsafe void ActionEffectDetour(uint casterEntityId, Character* casterPtr, FFXIVClientStructs.FFXIV.Common.Math.Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targetEntityIds)
	{
		_actionEffectHook.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);
		try
		{
			if (!ShouldCapture())
			{
				return;
			}
			uint spellId = header->SpellId;
			if (spellId == 0)
			{
				return;
			}
			IGameObject gameObject = Plugin.ObjectTable.SearchById(casterEntityId);
			if (gameObject == null || !(gameObject is IBattleChara bc))
			{
				return;
			}
			ActorKind actorKind = Classify(bc);
			if (actorKind == ActorKind.Other)
			{
				return;
			}
			Lumina.Excel.Sheets.Action? rowOrDefault = Plugin.Actions.GetRowOrDefault(spellId);
			if (!rowOrDefault.HasValue)
			{
				return;
			}
			Lumina.Excel.Sheets.Action valueOrDefault = rowOrDefault.GetValueOrDefault();
			if (valueOrDefault.ActionCategory.RowId == 1)
			{
				return;
			}
			string name = CleanName(valueOrDefault.Name.ExtractText(), spellId);
			int num = Math.Min((int)header->NumTargets, 32);
			uint[] array = new uint[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (uint)((ulong)targetEntityIds[i] & 0xFFFFFFFFu);
			}
			uint num2 = ((num > 0) ? array[0] : 0u);
			object obj;
			if (num2 != 0)
			{
				IGameObject gameObject2 = Plugin.ObjectTable.SearchById(num2);
				if (gameObject2 != null)
				{
					obj = gameObject2.Name.TextValue;
					goto IL_0141;
				}
			}
			obj = "";
			goto IL_0141;
			IL_0141:
			string targetName = (string)obj;
			QueueFromHook(new LogEvent
			{
				Kind = LogKind.Ability,
				SourceName = gameObject.Name.TextValue,
				SourceId = gameObject.EntityId,
				SourceKind = actorKind,
				TargetName = targetName,
				TargetId = num2,
				Name = name,
				DataId = spellId,
				IconId = valueOrDefault.Icon,
				X = gameObject.Position.X,
				Y = gameObject.Position.Z,
				Heading = gameObject.Rotation,
				AbilityTargetIds = array
			});
			if (targetPos != null)
			{
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.AbilityExtra,
					SourceId = gameObject.EntityId,
					SourceName = gameObject.Name.TextValue,
					SourceKind = actorKind,
					Name = name,
					DataId = spellId,
					X = targetPos->X,
					Y = targetPos->Z
				}, _config.ShowPositions);
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] action-effect error: " + ex.Message);
		}
	}

	private void ActorControlDetour(uint actorId, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, GameObjectId targetId, bool replaying)
	{
		_actorControlHook.Original(actorId, category, p1, p2, p3, p4, p5, p6, p7, p8, targetId, replaying);
		try
		{
			if (!ShouldCapture())
			{
				return;
			}
			switch (category)
			{
			case 34u:
			{
				if (p1 == 0)
				{
					_activeHeadmarkers.Remove(actorId);
				}
				else
				{
					_activeHeadmarkers[actorId] = p1;
				}
				IGameObject gameObject3 = Plugin.ObjectTable.SearchById(actorId);
				string sourceName = gameObject3?.Name.TextValue ?? "";
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.Headmarker,
					SourceId = actorId,
					SourceName = sourceName,
					SourceKind = ((gameObject3 is IBattleChara bc3) ? Classify(bc3) : ActorKind.Other),
					DataId = p1,
					Param1 = p2,
					Name = $"Headmarker {p1:X4}"
				});
				break;
			}
			case 407u:
			{
				IGameObject gameObject2 = Plugin.ObjectTable.SearchById(actorId);
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.TimelineEvent,
					SourceId = actorId,
					SourceName = (gameObject2?.Name.TextValue ?? ""),
					SourceKind = ((gameObject2 is IBattleChara bc2) ? Classify(bc2) : ActorKind.Other),
					DataId = p1,
					Name = $"Timeline {p1:X4}"
				});
				break;
			}
			case 184u:
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.ActorTargetVfx,
					SourceId = actorId,
					DataId = p1,
					Name = $"TargetVfx {p1:X4}"
				});
				break;
			case 413u:
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.EventObject,
					SourceId = actorId,
					Param1 = p1,
					Param2 = p2,
					Name = $"EventObject {p1}/{p2}"
				});
				break;
			default:
			{
				IGameObject gameObject = Plugin.ObjectTable.SearchById(actorId);
				QueueFromHook(new LogEvent
				{
					Kind = LogKind.ActorControl,
					SourceId = actorId,
					SourceName = (gameObject?.Name.TextValue ?? ""),
					SourceKind = ((gameObject is IBattleChara bc) ? Classify(bc) : ActorKind.Other),
					Name = "ActorControl",
					Category = category,
					Param1 = p1,
					Param2 = p2,
					Param3 = p3,
					Param4 = p4
				}, _config.ShowControl);
				break;
			}
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] actor-control error: " + ex.Message);
		}
	}

	private static ActorKind Classify(IBattleChara bc)
	{
		if (bc.EntityId == Plugin.PlayerState.EntityId)
		{
			return ActorKind.You;
		}
		if (bc.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc)
		{
			return ActorKind.Party;
		}
		if (bc.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc)
		{
			if (bc is IBattleNpc battleNpc && IsFriendlyNpc((byte)battleNpc.BattleNpcKind))
			{
				return ActorKind.Party;
			}
			return ActorKind.Enemy;
		}
		return ActorKind.Other;
	}

	private static bool IsFriendlyNpc(byte kind)
	{
		if ((uint)(kind - 2) <= 1u || kind == 9)
		{
			return true;
		}
		return false;
	}
}

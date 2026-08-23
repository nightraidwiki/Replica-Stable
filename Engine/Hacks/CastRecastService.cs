using System;
using Dalamud.Hooking;

namespace Replica.Engine.Hacks;

public sealed class CastRecastService : IDisposable
{
	private readonly Plugin _plugin;

	// Signatures for Cast Time calculation and progress
	private const string GetCastTimeSig = "e8 ?? ?? ?? ?? 45 ?? ?? 33 ?? 48 ?? ?? 66 ?? ?? ??";
	private const string CastProgressSig = "48 89 5C 24 ?? 57 48 83 EC ?? 48 8B F9 0F 29 74 24 ?? 0F B6 49";
	private const string CastBaseStaticSig = "F3 44 0F 2C C0 BA ?? ?? ?? ?? 48 8B CB E8 ?? ?? ?? ?? F3 44 0F 10 1D";

	// Signatures for Recast / Global Cooldown calculation
	private const string GetRecastTimeSig = "48 89 5c 24 ?? 48 89 74 24 ?? 55 57 41 ?? 41 ?? 41 ?? 48 ?? ?? 48 ?? ?? ?? 4c ?? ?? ?? ?? ?? ??";

	// Ninja Mudra action IDs: Ten (2259, 18805), Chi (2261, 18806), Jin (2263, 18807)
	private static readonly uint[] MudraActionIds = [2259, 2261, 2263, 18805, 18806, 18807];

	private unsafe delegate int GetCastTimeDelegate(uint actionType, uint actionId, bool applyProcess, byte* castTimeProc);
	private Hook<GetCastTimeDelegate>? _getCastTimeHook;

	private delegate uint CastProgressDelegate(nint data, uint spellActionId, float process, float processTotal);
	private Hook<CastProgressDelegate>? _castProgressHook;
	private unsafe float* _castProgressStaticAddr;

	private delegate long GetRecastTimeDelegate(int type, int key, char extra);
	private Hook<GetRecastTimeDelegate>? _getRecastTimeHook;

	public bool IsAvailable => _getCastTimeHook != null || _getRecastTimeHook != null;

	public bool DecCastEnabled
	{
		get => _plugin.Configuration.DecCastEnabled;
		set
		{
			if (_plugin.Configuration.DecCastEnabled != value)
			{
				_plugin.Configuration.DecCastEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public float DecCastTime
	{
		get => Math.Clamp(_plugin.Configuration.DecCastTime, 0.0f, 10.0f);
		set
		{
			float clamped = Math.Clamp(MathF.Round(value * 10f) / 10f, 0.0f, 10.0f);
			if (Math.Abs(_plugin.Configuration.DecCastTime - clamped) > 0.001f)
			{
				_plugin.Configuration.DecCastTime = clamped;
				_plugin.Configuration.Save();
			}
		}
	}

	public bool DecRecastEnabled
	{
		get => _plugin.Configuration.DecRecastEnabled;
		set
		{
			if (_plugin.Configuration.DecRecastEnabled != value)
			{
				_plugin.Configuration.DecRecastEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public float DecRecastTime
	{
		get => Math.Clamp(_plugin.Configuration.DecRecastTime, 0.0f, 10.0f);
		set
		{
			float clamped = Math.Clamp(MathF.Round(value * 10f) / 10f, 0.0f, 10.0f);
			if (Math.Abs(_plugin.Configuration.DecRecastTime - clamped) > 0.001f)
			{
				_plugin.Configuration.DecRecastTime = clamped;
				_plugin.Configuration.Save();
			}
		}
	}

	public bool MudraNoRecastEnabled
	{
		get => _plugin.Configuration.MudraNoRecastEnabled;
		set
		{
			if (_plugin.Configuration.MudraNoRecastEnabled != value)
			{
				_plugin.Configuration.MudraNoRecastEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public unsafe CastRecastService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			_getCastTimeHook = Plugin.GameInterop.HookFromSignature<GetCastTimeDelegate>(GetCastTimeSig, OnGetCastTimeDetour);
			Plugin.Log?.Information("[Replica] CastRecastService GetCastTime Hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize GetCastTime Hook: {ex.Message}");
		}

		try
		{
			_castProgressHook = Plugin.GameInterop.HookFromSignature<CastProgressDelegate>(CastProgressSig, OnCastProgressDetour);

			nint staticAddr = Plugin.SigScanner.GetStaticAddressFromSig(CastBaseStaticSig, 18);
			if (staticAddr != nint.Zero)
			{
				_castProgressStaticAddr = (float*)staticAddr;
			}
			Plugin.Log?.Information("[Replica] CastRecastService CastProgress Hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize CastProgress Hook: {ex.Message}");
		}

		try
		{
			_getRecastTimeHook = Plugin.GameInterop.HookFromSignature<GetRecastTimeDelegate>(GetRecastTimeSig, OnGetRecastTimeDetour);
			Plugin.Log?.Information("[Replica] CastRecastService Recast Hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize Recast Hook: {ex.Message}");
		}

		UpdateHookState();
	}

	public void UpdateHookState()
	{
		try
		{
			bool castActive = _plugin.Configuration.HacksUnlocked && DecCastEnabled;
			if (castActive)
			{
				if (_getCastTimeHook != null && !_getCastTimeHook.IsEnabled)
					_getCastTimeHook.Enable();
				if (_castProgressHook != null && !_castProgressHook.IsEnabled)
					_castProgressHook.Enable();
			}
			else
			{
				if (_getCastTimeHook != null && _getCastTimeHook.IsEnabled)
					_getCastTimeHook.Disable();
				if (_castProgressHook != null && _castProgressHook.IsEnabled)
					_castProgressHook.Disable();
			}

			bool recastActive = _plugin.Configuration.HacksUnlocked && (DecRecastEnabled || MudraNoRecastEnabled);
			if (recastActive)
			{
				if (_getRecastTimeHook != null && !_getRecastTimeHook.IsEnabled)
					_getRecastTimeHook.Enable();
			}
			else
			{
				if (_getRecastTimeHook != null && _getRecastTimeHook.IsEnabled)
					_getRecastTimeHook.Disable();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating CastRecast hook state: {ex.Message}");
		}
	}

	private unsafe int OnGetCastTimeDetour(uint actionType, uint actionId, bool applyProcess, byte* castTimeProc)
	{
		int original = _getCastTimeHook!.Original(actionType, actionId, applyProcess, castTimeProc);
		if (!_plugin.Configuration.HacksUnlocked || !DecCastEnabled || original <= 0)
		{
			return original;
		}

		return Math.Max(original - (int)(DecCastTime * 1000f), 0);
	}

	private unsafe uint OnCastProgressDetour(nint data, uint spellActionId, float process, float processTotal)
	{
		if (_plugin.Configuration.HacksUnlocked && DecCastEnabled && data != nint.Zero)
		{
			try
			{
				if (*(uint*)(data + 4) == spellActionId && _castProgressStaticAddr != null)
				{
					process = Math.Max(process - DecCastTime, 0f);
					*_castProgressStaticAddr = process;
				}
			}
			catch
			{
				// Defensive check
			}
		}

		return _castProgressHook!.Original(data, spellActionId, process, processTotal);
	}

	private long OnGetRecastTimeDetour(int type, int key, char extra)
	{
		long original = _getRecastTimeHook!.Original(type, key, extra);
		if (!_plugin.Configuration.HacksUnlocked)
		{
			return original;
		}

		if (original != 0L && type == 1)
		{
			if (MudraNoRecastEnabled && Array.IndexOf(MudraActionIds, (uint)key) >= 0)
			{
				return 0L;
			}

			if (DecRecastEnabled)
			{
				return (long)Math.Max((float)original - DecRecastTime * 1000f, 0f);
			}
		}

		return original;
	}

	public void Dispose()
	{
		try
		{
			_getCastTimeHook?.Disable();
			_getCastTimeHook?.Dispose();
			_getCastTimeHook = null;

			_castProgressHook?.Disable();
			_castProgressHook?.Dispose();
			_castProgressHook = null;

			_getRecastTimeHook?.Disable();
			_getRecastTimeHook?.Dispose();
			_getRecastTimeHook = null;
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] CastRecast dispose: {ex.Message}");
		}
	}
}

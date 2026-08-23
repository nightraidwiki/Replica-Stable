using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Hooking;

namespace Replica.Engine.Hacks;

public sealed class SpeedService : IDisposable
{
	private readonly Plugin _plugin;

	// Decrypted SpeedAddress signature from I-Ching:
	// "40 57 48 83 EC ?? 48 8B F9 48 8B 49 ?? 48 8B 01 FF 90 ?? ?? ?? ?? 48 85 C0 75"
	private const string SpeedAddressSig = "40 57 48 83 EC ?? 48 8B F9 48 8B 49 ?? 48 8B 01 FF 90 ?? ?? ?? ?? 48 85 C0 75";

	// Decrypted Acceleration signature from I-Ching:
	// "40 ?? 48 ?? ?? ?? 80 79 ?? ?? 48 ?? ?? 0f 84 ?? ?? ?? ?? 48 89 7c 24 ?? 48 ?? ?? ??"
	private const string AccelerationSig = "40 ?? 48 ?? ?? ?? 80 79 ?? ?? 48 ?? ?? 0f 84 ?? ?? ?? ?? 48 89 7c 24 ?? 48 ?? ?? ??";

	// Statuses that prevent movement speed alteration (Heavy, Bind, Sleep, Stun, etc.)
	private static readonly HashSet<uint> BlacklistedStatuses =
	[
		14u, 67u, 181u, 240u, 436u, 484u, 502u, 623u, 674u, 709u,
		1073u, 1107u, 1114u, 1141u, 1147u, 1259u, 1344u, 1394u, 1595u, 1790u,
		1796u, 1935u, 2099u, 2158u, 2391u, 2551u, 2662u, 2731u, 3167u, 3284u,
		3472u, 3473u, 3548u, 3943u, 3948u, 4334u, 4341u
	];

	private delegate float GetMovementSpeedDelegate(nint self);
	private Hook<GetMovementSpeedDelegate>? _movementSpeedHook;

	private delegate void AccelerationDelegate(nint self);
	private Hook<AccelerationDelegate>? _accelerationHook;

	public bool IsAvailable => _movementSpeedHook != null;

	public bool IsActive => IsEnabled && _plugin.Configuration.HacksUnlocked;

	public bool IsEnabled
	{
		get => _plugin.Configuration.SpeedEnabled;
		set
		{
			_plugin.Configuration.SpeedEnabled = value;
			_plugin.Configuration.Save();
			UpdateHookState();
		}
	}

	public float SpeedValue
	{
		get => Math.Clamp(_plugin.Configuration.SpeedValue, 0.1f, 10.0f);
		set
		{
			float clamped = Math.Clamp(MathF.Round(value * 10f) / 10f, 0.1f, 10.0f);
			if (Math.Abs(_plugin.Configuration.SpeedValue - clamped) > 0.001f)
			{
				_plugin.Configuration.SpeedValue = clamped;
				_plugin.Configuration.Save();
			}
		}
	}

	public float SpeedMultiplier
	{
		get => SpeedValue;
		set => SpeedValue = value;
	}

	public bool MaxAcceleration
	{
		get => _plugin.Configuration.MaxAccelerationEnabled;
		set
		{
			_plugin.Configuration.MaxAccelerationEnabled = value;
			_plugin.Configuration.Save();
			UpdateHookState();
		}
	}

	public SpeedService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			_movementSpeedHook = Plugin.GameInterop.HookFromSignature<GetMovementSpeedDelegate>(SpeedAddressSig, OnMovementSpeedDetour);
			Plugin.Log?.Information($"[Replica] SpeedService Movement Speed Hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize SpeedService Movement Speed Hook: {ex.Message}");
		}

		try
		{
			_accelerationHook = Plugin.GameInterop.HookFromSignature<AccelerationDelegate>(AccelerationSig, OnAccelerationDetour);
			Plugin.Log?.Information($"[Replica] SpeedService Acceleration Hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize SpeedService Acceleration Hook: {ex.Message}");
		}

		UpdateHookState();
	}

	public void UpdateHookState()
	{
		try
		{
			if (IsActive && _movementSpeedHook != null)
			{
				if (!_movementSpeedHook.IsEnabled)
					_movementSpeedHook.Enable();
			}
			else
			{
				if (_movementSpeedHook != null && _movementSpeedHook.IsEnabled)
					_movementSpeedHook.Disable();
			}

			if (MaxAcceleration && _plugin.Configuration.HacksUnlocked && _accelerationHook != null)
			{
				if (!_accelerationHook.IsEnabled)
					_accelerationHook.Enable();
			}
			else
			{
				if (_accelerationHook != null && _accelerationHook.IsEnabled)
					_accelerationHook.Disable();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating SpeedService hook state: {ex.Message}");
		}
	}

	private float OnMovementSpeedDetour(nint self)
	{
		float original = _movementSpeedHook!.Original(self);
		if (!IsEnabled || !_plugin.Configuration.HacksUnlocked)
		{
			return original;
		}

		IPlayerCharacter? localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer == null)
		{
			return original;
		}

		// Filter blocking movement debuffs in PvP or Deep Dungeons
		if (Plugin.Condition[ConditionFlag.InDeepDungeon] || Plugin.ClientState.IsPvP)
		{
			try
			{
				foreach (IStatus status in localPlayer.StatusList)
				{
					if (BlacklistedStatuses.Contains(status.StatusId))
					{
						return original;
					}
				}
			}
			catch
			{
				// Defensive fallback
			}
		}

		// I-Ching calculation: original + SpeedValue
		return original + SpeedValue;
	}

	private void OnAccelerationDetour(nint self)
	{
		if (MaxAcceleration && _plugin.Configuration.HacksUnlocked && self != nint.Zero)
		{
			try
			{
				Dalamud.SafeMemory.Write(self + 68, 100f);
			}
			catch
			{
				// Defensive fallback
			}
		}

		_accelerationHook!.Original(self);
	}

	public bool Toggle()
	{
		IsEnabled = !IsEnabled;
		return IsEnabled;
	}

	public void Dispose()
	{
		try
		{
			if (_movementSpeedHook != null)
			{
				if (_movementSpeedHook.IsEnabled)
					_movementSpeedHook.Disable();
				_movementSpeedHook.Dispose();
			}
			if (_accelerationHook != null)
			{
				if (_accelerationHook.IsEnabled)
					_accelerationHook.Disable();
				_accelerationHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] SpeedService dispose: {ex.Message}");
		}
		_movementSpeedHook = null;
		_accelerationHook = null;
	}
}

using System;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Replica.Engine.Interop;

namespace Replica.Engine.Hacks;

public sealed class LocalFlightService : IDisposable
{
	private readonly Plugin _plugin;

	// Hook signatures
	private const string FlightAllowedStatusSig = "40 53 48 83 EC ?? 48 8B 1D ?? ?? ?? ?? 48 85 DB 0F 84 ?? ?? ?? ?? 80 3D";
	
	private const string AetherCurrentInterruptSig = "0F 84 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 05";

	// Hook delegates (GetFlightAllowedStatus takes no arguments in FFXIV)
	private delegate Control.FlightAllowedStatus GetFlightAllowedStatusDelegate();
	private Hook<GetFlightAllowedStatusDelegate>? _flightAllowedStatusHook;

	[return: MarshalAs(UnmanagedType.U1)]
	private delegate bool ExecuteCommandDelegate(int commandId, int param1, int param2, int param3, int param4);
	private Hook<ExecuteCommandDelegate>? _executeCommandHook;

	// Memory Patch variables
	private nint _patchAddress = nint.Zero;
	private byte[]? _originalBytes;
	private byte[]? _patchedBytes;

	public bool IsAvailable => _flightAllowedStatusHook != null;

	public bool IsEnabled
	{
		get => _plugin.Configuration.LocalFlightEnabled;
		set
		{
			_plugin.Configuration.LocalFlightEnabled = value;
			_plugin.Configuration.Save();
			UpdateHookState();
		}
	}

	public bool ProhibitFlightRestrictions
	{
		get => _plugin.Configuration.ProhibitFlightRestrictionsEnabled;
		set
		{
			_plugin.Configuration.ProhibitFlightRestrictionsEnabled = value;
			_plugin.Configuration.Save();
			UpdateHookState();
		}
	}

	public LocalFlightService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			_flightAllowedStatusHook = Svc.Hook.HookFromSignature<GetFlightAllowedStatusDelegate>(FlightAllowedStatusSig, OnFlightAllowedStatusDetour);
			Plugin.Log?.Information("[Replica] LocalFlightService GetFlightAllowedStatus hook initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize LocalFlightService hook: {ex.Message}");
		}

		try
		{
			nint executeCmdPtr = (nint)FFXIVClientStructs.FFXIV.Client.Game.GameMain.Addresses.ExecuteCommand.Value;
			if (executeCmdPtr != nint.Zero)
			{
				_executeCommandHook = Svc.Hook.HookFromAddress<ExecuteCommandDelegate>(executeCmdPtr, OnExecuteCommandDetour);
				Plugin.Log?.Information("[Replica] LocalFlightService ExecuteCommand hook initialized.");
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize ExecuteCommand hook in LocalFlightService: {ex.Message}");
		}

		InitializeMemoryPatch();
		UpdateHookState();
	}

	private void InitializeMemoryPatch()
	{
		try
		{
			if (Svc.SigScanner.TryScanText(AetherCurrentInterruptSig, out _patchAddress))
			{
				byte[] original = new byte[6];
				Marshal.Copy(_patchAddress, original, 0, 6);
				_originalBytes = original;

				if (original[0] == 0x0F && original[1] == 0x84)
				{
					int offset = BitConverter.ToInt32(original, 2);
					int newOffset = offset + 1;
					byte[] newOffsetBytes = BitConverter.GetBytes(newOffset);

					_patchedBytes = new byte[6];
					_patchedBytes[0] = 0xE9; // jmp
					Array.Copy(newOffsetBytes, 0, _patchedBytes, 1, 4);
					_patchedBytes[5] = 0x90; // nop
					
					Plugin.Log?.Information($"[Replica] Initialized aetherCurrentInterruptPatch dynamically at 0x{_patchAddress:X}. Offset: 0x{offset:X} -> 0x{newOffset:X}.");
				}
				else
				{
					_patchedBytes = new byte[] { 0xE9, 0x9D, 0x16, 0x00, 0x00, 0x90 };
					Plugin.Log?.Warning($"[Replica] Unexpected bytes at aetherCurrentInterruptPatch signature. Using fallback patch.");
				}
			}
			else
			{
				Plugin.Log?.Warning("[Replica] Could not find aetherCurrentInterruptPatch signature.");
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error initializing memory patch: {ex.Message}");
		}
	}

	public void UpdateHookState()
	{
		try
		{
			bool isUnlocked = _plugin.Configuration.HacksUnlocked;
			bool localFlightActive = IsEnabled && isUnlocked;
			bool restrictionsActive = ProhibitFlightRestrictions && isUnlocked;

			// Update FlightAllowedStatus Hook
			if ((localFlightActive || restrictionsActive) && _flightAllowedStatusHook != null)
			{
				if (!_flightAllowedStatusHook.IsEnabled)
					_flightAllowedStatusHook.Enable();
			}
			else
			{
				if (_flightAllowedStatusHook != null && _flightAllowedStatusHook.IsEnabled)
					_flightAllowedStatusHook.Disable();
			}

			// Update ExecuteCommand Hook
			if (restrictionsActive && _executeCommandHook != null)
			{
				if (!_executeCommandHook.IsEnabled)
					_executeCommandHook.Enable();
			}
			else
			{
				if (_executeCommandHook != null && _executeCommandHook.IsEnabled)
					_executeCommandHook.Disable();
			}

			// Apply/Remove memory patch
			ApplyPatch(restrictionsActive);
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating LocalFlightService hook state: {ex.Message}");
		}
	}

	private void ApplyPatch(bool enable)
	{
		if (_patchAddress == nint.Zero || _originalBytes == null || _patchedBytes == null)
			return;

		try
		{
			SafeMemory.WriteBytes(_patchAddress, enable ? _patchedBytes : _originalBytes);
			Plugin.Log?.Debug($"[Replica] {(enable ? "Applied" : "Removed")} aetherCurrentInterruptPatch.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Failed to {(enable ? "apply" : "remove")} aetherCurrentInterruptPatch: {ex.Message}");
		}
	}

	private Control.FlightAllowedStatus OnFlightAllowedStatusDetour()
	{
		Control.FlightAllowedStatus original = _flightAllowedStatusHook!.Original();
		
		bool isUnlocked = _plugin.Configuration.HacksUnlocked;

		if (IsEnabled && isUnlocked)
		{
			return Control.FlightAllowedStatus.CanFly; // 0
		}

		if (ProhibitFlightRestrictions && isUnlocked)
		{
			if (original == Control.FlightAllowedStatus.Unk1) // 1
			{
				return Control.FlightAllowedStatus.CanFly; // 0
			}
		}

		return original;
	}

	private bool OnExecuteCommandDetour(int commandId, int param1, int param2, int param3, int param4)
	{
		bool isUnlocked = _plugin.Configuration.HacksUnlocked;

		if (ProhibitFlightRestrictions && isUnlocked && commandId == 612) // DisableMounting
		{
			if (param1 == 1)
			{
				return true; // block command
			}
			param1 = 0;
		}

		return _executeCommandHook!.Original(commandId, param1, param2, param3, param4);
	}

	public void Dispose()
	{
		try
		{
			ApplyPatch(false);

			if (_flightAllowedStatusHook != null)
			{
				if (_flightAllowedStatusHook.IsEnabled)
					_flightAllowedStatusHook.Disable();
				_flightAllowedStatusHook.Dispose();
			}

			if (_executeCommandHook != null)
			{
				if (_executeCommandHook.IsEnabled)
					_executeCommandHook.Disable();
				_executeCommandHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] LocalFlightService dispose: {ex.Message}");
		}
		_flightAllowedStatusHook = null;
		_executeCommandHook = null;
	}
}

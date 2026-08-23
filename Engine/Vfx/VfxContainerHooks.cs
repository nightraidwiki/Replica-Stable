using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Replica.Logging;

namespace Replica.Engine.Vfx;

internal static class VfxContainerHooks
{
	private unsafe delegate long TetherCreateDelegate(VfxContainer* container, byte a2, ushort tetherId, ulong targetOid, byte a5);

	private unsafe delegate long TetherCancelDelegate(VfxContainer* container, byte a2, ushort a3, byte a4, byte a5);

	private const uint InvalidTargetOid = 3758096384u;

	private static Hook<TetherCreateDelegate>? _createHook;

	private static Hook<TetherCancelDelegate>? _cancelHook;

	private static CombatLogCapture? _capture;

	private const string CreateSig = "48 89 5C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 20 0F B6 74 24";

	private const string CancelSig = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 F2 41 0F B6 E9";

	public static bool Installed { get; private set; }

	public static string InstallError { get; private set; } = "";

	public unsafe static void Init(CombatLogCapture capture, IGameInteropProvider interop, ISigScanner sigScanner, IPluginLog log)
	{
		_capture = capture;
		try
		{
			_createHook = interop.HookFromAddress<TetherCreateDelegate>(sigScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 20 0F B6 74 24"), TetherCreateDetour);
			_createHook.Enable();
			_cancelHook = interop.HookFromAddress<TetherCancelDelegate>(sigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 F2 41 0F B6 E9"), TetherCancelDetour);
			_cancelHook.Enable();
			Installed = true;
		}
		catch (Exception ex)
		{
			InstallError = ex.Message;
			log.Information("[Replica] VfxContainer tether hooks unavailable on this build: " + ex.Message);
		}
	}

	public static void SetEnabled(bool enabled)
	{
		try
		{
			if (enabled)
			{
				_createHook?.Enable();
			}
			else
			{
				_createHook?.Disable();
			}
		}
		catch
		{
		}
		try
		{
			if (enabled)
			{
				_cancelHook?.Enable();
			}
			else
			{
				_cancelHook?.Disable();
			}
		}
		catch
		{
		}
	}

	public static void Dispose()
	{
		try
		{
			_createHook?.Dispose();
		}
		catch
		{
		}
		try
		{
			_cancelHook?.Dispose();
		}
		catch
		{
		}
		_createHook = null;
		_cancelHook = null;
		_capture = null;
		Installed = false;
	}

	private unsafe static long TetherCreateDetour(VfxContainer* container, byte a2, ushort tetherId, ulong targetOid, byte a5)
	{
		long result = _createHook.Original(container, a2, tetherId, targetOid, a5);
		try
		{
			if (container == null)
			{
				return result;
			}
			Character* ownerObject = container->OwnerObject;
			if (ownerObject == null || targetOid == 3758096384u)
			{
				return result;
			}
			_capture?.NotifyTetherFromVfx(ownerObject->EntityId, (uint)targetOid, tetherId);
		}
		catch
		{
		}
		return result;
	}

	private unsafe static long TetherCancelDetour(VfxContainer* container, byte a2, ushort a3, byte a4, byte a5)
	{
		long result = _cancelHook.Original(container, a2, a3, a4, a5);
		try
		{
			if (container == null)
			{
				return result;
			}
			Character* ownerObject = container->OwnerObject;
			if (ownerObject == null)
			{
				return result;
			}
			_capture?.NotifyTetherCancelFromVfx(ownerObject->EntityId);
		}
		catch
		{
		}
		return result;
	}
}

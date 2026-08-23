using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Hooking;
using Replica.Engine.Interop;

namespace Replica.Engine.Hacks;

public sealed class NoClipService : IDisposable
{
	private readonly Plugin _plugin;

	// Hook signature for CalculateCollision in FFXIV
	private const string CalculateCollisionSig = "48 89 74 24 ?? 48 89 7C 24 ?? 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F3 0F 10 42";

	// Hook delegate matching the signature's parameters
	private unsafe delegate nint CalculateCollisionDelegate(
		nint moveControlInstance,
		Vector3* expectedPosition,
		nint controlState,
		Vector3* currentPosition,
		nint collisionFlags,
		ushort movementType
	);

	private Hook<CalculateCollisionDelegate>? _calculateCollisionHook;

	public bool IsAvailable => _calculateCollisionHook != null;

	public bool IsEnabled
	{
		get => _plugin.Configuration.NoClipEnabled;
		set
		{
			_plugin.Configuration.NoClipEnabled = value;
			_plugin.Configuration.Save();
			UpdateHookState();
		}
	}

	public NoClipService(Plugin plugin)
	{
		_plugin = plugin;

		unsafe
		{
			try
			{
				_calculateCollisionHook = Svc.Hook.HookFromSignature<CalculateCollisionDelegate>(CalculateCollisionSig, OnCalculateCollisionDetour);
				Plugin.Log?.Information("[Replica] NoClipService CalculateCollision hook initialized.");
			}
			catch (Exception ex)
			{
				Plugin.Log?.Warning($"[Replica] Could not initialize NoClipService hook: {ex.Message}");
			}
		}

		UpdateHookState();
	}

	public void UpdateHookState()
	{
		try
		{
			bool isUnlocked = _plugin.Configuration.HacksUnlocked;
			bool active = IsEnabled && isUnlocked;

			if (active && _calculateCollisionHook != null)
			{
				if (!_calculateCollisionHook.IsEnabled)
					_calculateCollisionHook.Enable();
			}
			else
			{
				if (_calculateCollisionHook != null && _calculateCollisionHook.IsEnabled)
					_calculateCollisionHook.Disable();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating NoClipService hook state: {ex.Message}");
		}
	}

	private unsafe nint OnCalculateCollisionDetour(
		nint moveControlInstance,
		Vector3* expectedPosition,
		nint controlState,
		Vector3* currentPosition,
		nint collisionFlags,
		ushort movementType)
	{
		bool isUnlocked = _plugin.Configuration.HacksUnlocked;
		if (IsEnabled && isUnlocked)
		{
			return nint.Zero; // bypass collision
		}

		return _calculateCollisionHook!.Original(moveControlInstance, expectedPosition, controlState, currentPosition, collisionFlags, movementType);
	}

	public void Dispose()
	{
		try
		{
			if (_calculateCollisionHook != null)
			{
				if (_calculateCollisionHook.IsEnabled)
					_calculateCollisionHook.Disable();
				_calculateCollisionHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] NoClipService dispose: {ex.Message}");
		}
		_calculateCollisionHook = null;
	}
}

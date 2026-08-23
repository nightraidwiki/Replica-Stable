using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;

namespace Replica.Engine.Hacks;

public sealed class SlidecastService : IDisposable
{
	private readonly Plugin _plugin;

	// Signature for ZoneClient::SendPacket call matching I-Ching NetRe
	private const string SendPacketSig = "e8 ?? ?? ?? ?? 84 ?? 74 ?? 48 ?? ?? c7 87 ?? ?? ?? ?? ?? ?? ?? ??";

	// Signatures to dynamically extract position update packet opcodes
	private const string UpdatePositionInstanceSig = "41 B8 ?? ?? ?? ?? F6 C2";
	private const string UpdatePositionHandlerSig = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B F9 41 8B D8";

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe delegate bool SendPacketDelegate(nint client, nint packet, uint a3, uint a4, bool priority);

	private Hook<SendPacketDelegate>? _sendPacketHook;

	private uint _updatePositionInstance;
	private uint _updatePositionHandler;

	public bool IsAvailable { get; private set; }

	public uint UpdatePositionInstanceOpcode => _updatePositionInstance;
	public uint UpdatePositionHandlerOpcode => _updatePositionHandler;

	public bool IsActive => IsEnabled && _plugin.Configuration.HacksUnlocked;

	public bool IsEnabled
	{
		get => _plugin.Configuration.SlidecastEnabled;
		set
		{
			if (_plugin.Configuration.SlidecastEnabled != value)
			{
				_plugin.Configuration.SlidecastEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public float SlidecastWindow
	{
		get => Math.Clamp(_plugin.Configuration.SlidecastWindow, 0.0f, 1.0f);
		set
		{
			float clamped = Math.Clamp(MathF.Round(value * 10f) / 10f, 0.0f, 1.0f);
			if (Math.Abs(_plugin.Configuration.SlidecastWindow - clamped) > 0.001f)
			{
				_plugin.Configuration.SlidecastWindow = clamped;
				_plugin.Configuration.Save();
			}
		}
	}

	public bool IsSuppressingMovement { get; private set; }
	public ulong SuppressedPacketsCount { get; private set; }

	public SlidecastService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			nint instanceAddr = Plugin.SigScanner.ScanText(UpdatePositionInstanceSig);
			if (instanceAddr != nint.Zero)
			{
				Dalamud.SafeMemory.Read<uint>(instanceAddr + 2, out _updatePositionInstance);
			}

			nint handlerAddr = Plugin.SigScanner.ScanText(UpdatePositionHandlerSig);
			if (handlerAddr != nint.Zero)
			{
				Dalamud.SafeMemory.Read<uint>(handlerAddr + 81, out _updatePositionHandler);
			}

			_sendPacketHook = Plugin.GameInterop.HookFromSignature<SendPacketDelegate>(SendPacketSig, OnSendPacketDetour);
			IsAvailable = _sendPacketHook != null && _updatePositionInstance != 0 && _updatePositionHandler != 0;

			Plugin.Log?.Information($"[Replica] SlidecastService initialized. Handler: 0x{_updatePositionHandler:X4}, Instance: 0x{_updatePositionInstance:X4}, Available: {IsAvailable}");
			UpdateHookState();
		}
		catch (Exception ex)
		{
			IsAvailable = false;
			Plugin.Log?.Warning($"[Replica] Could not initialize SlidecastService hook: {ex.Message}");
		}
	}

	public void UpdateHookState()
	{
		try
		{
			if (IsActive && _sendPacketHook != null)
			{
				if (!_sendPacketHook.IsEnabled)
				{
					_sendPacketHook.Enable();
				}
			}
			else
			{
				if (_sendPacketHook != null && _sendPacketHook.IsEnabled)
				{
					_sendPacketHook.Disable();
				}
				IsSuppressingMovement = false;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating SlidecastService hook state: {ex.Message}");
		}
	}

	private unsafe bool OnSendPacketDetour(nint client, nint packet, uint a3, uint a4, bool priority)
	{
		if (packet != nint.Zero && IsActive)
		{
			try
			{
				ushort opcode = *(ushort*)packet;
				IPlayerCharacter? player = Plugin.ObjectTable.LocalPlayer;
				if (player != null && player.IsCasting)
				{
					float remaining = player.TotalCastTime - player.CurrentCastTime;
					if (remaining <= SlidecastWindow && (opcode == _updatePositionHandler || opcode == _updatePositionInstance))
					{
						IsSuppressingMovement = true;
						SuppressedPacketsCount++;
						// Drop position packet to prevent server from canceling the active cast
						return true;
					}
				}
			}
			catch
			{
				// Defensive catch
			}
		}

		IsSuppressingMovement = false;
		return _sendPacketHook!.Original(client, packet, a3, a4, priority);
	}

	public bool Toggle()
	{
		IsEnabled = !IsEnabled;
		return IsEnabled;
	}

	public void ResetStats()
	{
		SuppressedPacketsCount = 0;
	}

	public void Dispose()
	{
		IsSuppressingMovement = false;
		try
		{
			if (_sendPacketHook != null)
			{
				if (_sendPacketHook.IsEnabled)
				{
					_sendPacketHook.Disable();
				}
				_sendPacketHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] SlidecastService dispose: {ex.Message}");
		}
		_sendPacketHook = null;
	}
}

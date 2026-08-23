using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using InteropGenerator.Runtime;
using Penumbra.String;
using Penumbra.String.Classes;
using Replica.Engine.Interop;
using Replica.Engine.Managers;

namespace Replica.Engine.Memory;

public sealed class ResourceService : IDisposable
{
	public unsafe delegate ResourceHandle* GetResourceSyncDelegate(ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path, void* unk, void* unkDebugPtr, uint unkDebugInt);

	public unsafe delegate ResourceHandle* GetResourceAsyncDelegate(ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path, void* unk, bool isUnknown, void* unkDebugPtr, uint unkDebugInt);

	private sealed class Crc32
	{
		private readonly uint[] _table = Enumerable.Range(0, 256).Select(delegate(int i)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) != 0) ? ((num >> 1) ^ 0xEDB88320u) : (num >> 1));
			}
			return num;
		}).ToArray();

		private uint _crc = uint.MaxValue;

		public uint Checksum => ~_crc;

		public void Init()
		{
			_crc = uint.MaxValue;
		}

		public void Update(byte b)
		{
			_crc = _table[(_crc ^ b) & 0xFF] ^ (_crc >> 8);
		}
	}

	private const string SigGetResourceSync = "E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81";

	private const string SigGetResourceAsync = "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00";

	private Crc32? _crc32;

	private Hook<GetResourceSyncDelegate>? _syncHook;

	private Hook<GetResourceAsyncDelegate>? _asyncHook;

	public unsafe void Init()
	{
		_syncHook = Svc.Hook.HookFromAddress<GetResourceSyncDelegate>(Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81"), SyncDetour);
		_syncHook.Enable();
		_asyncHook = Svc.Hook.HookFromAddress<GetResourceAsyncDelegate>(Svc.SigScanner.ScanText("E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00"), AsyncDetour);
		_asyncHook.Enable();
		_crc32 = new Crc32();
	}

	public void SetEnabled(bool enabled)
	{
		try
		{
			if (enabled)
			{
				_syncHook?.Enable();
			}
			else
			{
				_syncHook?.Disable();
			}
		}
		catch
		{
		}
		try
		{
			if (enabled)
			{
				_asyncHook?.Enable();
			}
			else
			{
				_asyncHook?.Disable();
			}
		}
		catch
		{
		}
	}

	public void Dispose()
	{
		_syncHook?.Dispose();
		_asyncHook?.Dispose();
		_syncHook = null;
		_asyncHook = null;
	}

	private unsafe ResourceHandle* SyncDetour(ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path, void* unk, void* unkDebugPtr, uint unkDebugInt)
	{
		return GetResourceHandler(isSync: true, resourceManager, category, type, hash, path, unk, isUnknown: false, unkDebugPtr, unkDebugInt);
	}

	private unsafe ResourceHandle* AsyncDetour(ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, CStringPointer path, void* unk, bool isUnknown, void* unkDebugPtr, uint unkDebugInt)
	{
		return GetResourceHandler(isSync: false, resourceManager, category, type, hash, path, unk, isUnknown, unkDebugPtr, unkDebugInt);
	}

	private unsafe ResourceHandle* GetResourceHandler(bool isSync, ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash, byte* path, void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt)
	{
		if (!Utf8GamePath.FromPointer(path, MetaDataComputation.CiCrc32, out var path2))
		{
			if (!isSync)
			{
				return _asyncHook.Original(resourceManager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
			}
			return _syncHook.Original(resourceManager, category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
		}
		string item = path2.ToString();
		if (!VfxBlocker.BlockedPaths.Contains(item))
		{
			if (!isSync)
			{
				return _asyncHook.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
			}
			return _syncHook.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
		}
		byte[] bytes = Encoding.ASCII.GetBytes("vfx/path/nothing.avfx");
		byte* ptr = stackalloc byte[(int)(uint)(bytes.Length + 1)];
		Marshal.Copy(bytes, 0, (nint)ptr, bytes.Length);
		path = ptr;
		_crc32.Init();
		byte[] array = bytes;
		foreach (byte b in array)
		{
			_crc32.Update(b);
		}
		*hash = _crc32.Checksum;
		if (!isSync)
		{
			return _asyncHook.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
		}
		return _syncHook.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
	}
}

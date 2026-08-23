using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Replica.Engine.Interop;

namespace Replica.Engine.Struct.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 278528)]
public struct VFXList : IEnumerable<VFXListData>, IEnumerable
{
	private static nint _getListFn = IntPtr.Zero;

	private static nint _scanAddress = IntPtr.Zero;

	private static int _listOffset = 0;

	private static bool _fnResolved;

	private static bool _addressScanned;

	private static bool _offsetResolved;

	public static HashSet<nint> vfxHandlesSet = new HashSet<nint>();

	[FieldOffset(0)]
	private unsafe fixed byte _buffer[278528];

	public unsafe Span<VFXListData> ListSpan
	{
		get
		{
			fixed (byte* buffer = _buffer)
			{
				return new Span<VFXListData>(buffer, 2048);
			}
		}
	}

	public static bool CheckVFXHandleExists(nint vfxHandle)
	{
		return vfxHandlesSet.Contains(vfxHandle);
	}

	public unsafe static void SyncVfxHandles()
	{
		vfxHandlesSet.Clear();
		VFXList* ptr = Instance();
		if (ptr == null)
		{
			return;
		}
		try
		{
			Span<VFXListData> listSpan = ptr->ListSpan;
			for (int i = 0; i < listSpan.Length; i++)
			{
				VFXListData vFXListData = listSpan[i];
				if (vFXListData.IsValid())
				{
					vfxHandlesSet.Add(vFXListData.VFXHandle);
				}
			}
		}
		catch (Exception e)
		{
			e.Log();
		}
	}

	public unsafe VFXListData* GetVFXListDataByIndex(int index)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, "index");
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, 2048, "index");
		return (VFXListData*)Unsafe.AsPointer(in ListSpan[index]);
	}

	public unsafe static VFXList* Instance()
	{
		nint listPointer = GetListPointer();
		if (IsInvalidPointer(listPointer))
		{
			return null;
		}
		nint num = *(nint*)(listPointer + ResolveListOffset());
		if (IsInvalidPointer(num))
		{
			return null;
		}
		nint num2 = num + 8192;
		if (IsInvalidPointer(num2))
		{
			return null;
		}
		return (VFXList*)num2;
	}

	private unsafe static nint GetListPointer()
	{
		ResolveFunction();
		if (IsInvalidPointer(_getListFn))
		{
			return IntPtr.Zero;
		}
		try
		{
			return ((delegate* unmanaged[Stdcall]<nint>)_getListFn)();
		}
		catch
		{
			return IntPtr.Zero;
		}
	}

	private unsafe static void ResolveFunction()
	{
		if (_fnResolved)
		{
			return;
		}
		_fnResolved = true;
		nint num = ScanFunction();
		if (IsInvalidPointer(num))
		{
			return;
		}
		try
		{
			byte* ptr = (byte*)num;
			if (ptr[6] == 232)
			{
				int num2 = *(int*)(ptr + 7);
				nint num3 = num + 11 + num2;
				if (!IsInvalidPointer(num3))
				{
					_getListFn = num3;
				}
			}
		}
		catch
		{
			_getListFn = IntPtr.Zero;
		}
	}

	private static nint ScanFunction()
	{
		if (_addressScanned)
		{
			return _scanAddress;
		}
		_addressScanned = true;
		nint result = 0;
		if (Svc.SigScanner.TryScanText("40 53 48 83 ec 20 e8 ?? ?? ?? ?? 45 33 c9 4c 8d 05 ?? ?? ?? ?? 48 8b d0 48 8b c8 48 8b d8 e8 ?? ?? ?? ?? 48 8b 8b ?? ?? ?? ?? 48 83 c4 20 5b e9 ?? ?? ?? ??", out result))
		{
			_scanAddress = result;
		}
		return _scanAddress;
	}

	private unsafe static int ResolveListOffset()
	{
		if (_listOffset != 0)
		{
			return _listOffset;
		}
		_listOffset = 5272;
		if (!_offsetResolved)
		{
			_offsetResolved = true;
			nint num = ScanFunction();
			if (!IsInvalidPointer(num))
			{
				try
				{
					byte* ptr = (byte*)num;
					if (ptr[14] == 76 && ptr[15] == 141 && ptr[16] == 5)
					{
						int num2 = *(int*)(ptr + 17);
						nint num3 = num + 21 + num2;
						if (!IsInvalidPointer(num3))
						{
							byte* ptr2 = (byte*)num3;
							for (int i = 0; i <= 280; i++)
							{
								if (ptr2[i] == 73 && ptr2[i + 1] == 139 && ptr2[i + 2] == 133)
								{
									_listOffset = *(int*)(ptr2 + i + 3);
									break;
								}
							}
						}
					}
				}
				catch
				{
					_listOffset = 5272;
				}
			}
		}
		return _listOffset;
	}

	private static bool IsInvalidPointer(nint pointer)
	{
		long num = pointer;
		if (num >= 65536)
		{
			return num > 140737488355327L;
		}
		return true;
	}

	public IEnumerator<VFXListData> GetEnumerator()
	{
		VFXListData[] entries = ListSpan.ToArray();
		for (int i = 0; i < entries.Length; i++)
		{
			VFXListData vFXListData = entries[i];
			if (vFXListData.IsValid())
			{
				yield return vFXListData;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

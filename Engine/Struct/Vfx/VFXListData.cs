using System;
using System.Runtime.InteropServices;

namespace Replica.Engine.Struct.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 136)]
public struct VFXListData
{
	[FieldOffset(48)]
	public nint pControl;

	[FieldOffset(56)]
	public nint pCompiled;

	[FieldOffset(64)]
	public nint VFXHandle;

	public readonly bool IsValid()
	{
		if (pControl != IntPtr.Zero && pCompiled != IntPtr.Zero)
		{
			return VFXHandle != IntPtr.Zero;
		}
		return false;
	}
}

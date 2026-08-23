using System.Numerics;
using System.Runtime.InteropServices;

namespace Replica.Engine.Struct.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 192)]
public struct Apricot
{
	[FieldOffset(0)]
	public unsafe void* vtbl;

	[FieldOffset(8)]
	public nint pCompiled;

	[FieldOffset(16)]
	public nint Ref;

	[FieldOffset(24)]
	public nint OmenVFXHandle;

	[FieldOffset(32)]
	public unsafe fixed float Matrix[16];

	[FieldOffset(80)]
	public Vector3 Position;

	[FieldOffset(96)]
	public long Id;

	[FieldOffset(96)]
	public uint CRC;

	[FieldOffset(100)]
	public uint Index;

	[FieldOffset(104)]
	public float Unkf1;

	[FieldOffset(108)]
	public float Unkf2;

	[FieldOffset(176)]
	public int Time;

	[FieldOffset(180)]
	public byte Unk1;

	[FieldOffset(181)]
	public byte Unk2;

	[FieldOffset(183)]
	public byte NeedInit;

	[FieldOffset(160)]
	public Vector4 ColorScale;

	public bool HasInit()
	{
		return (NeedInit & 1) == 0;
	}
}

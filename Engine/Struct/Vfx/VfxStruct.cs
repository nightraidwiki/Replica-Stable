using System.Numerics;
using System.Runtime.InteropServices;

namespace Replica.Engine.Struct.Vfx;

[StructLayout(LayoutKind.Explicit)]
public struct VfxStruct
{
	[FieldOffset(56)]
	public byte Flags;

	[FieldOffset(80)]
	public Vector3 Position;

	[FieldOffset(96)]
	public Quat Rotation;

	[FieldOffset(112)]
	public Vector3 Scale;

	[FieldOffset(296)]
	public int ActorCaster;

	[FieldOffset(304)]
	public int ActorTarget;

	[FieldOffset(440)]
	public int StaticCaster;

	[FieldOffset(448)]
	public int StaticTarget;

	[FieldOffset(672)]
	public unsafe Apricot* Apricot;

	[FieldOffset(592)]
	public float Speed;

	[FieldOffset(608)]
	public Vector4 Color;
}

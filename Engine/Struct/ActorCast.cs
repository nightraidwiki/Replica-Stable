using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Replica.Engine.Struct;

[StructLayout(LayoutKind.Explicit)]
public struct ActorCast
{
	[FieldOffset(0)]
	public ushort ActionId;

	[FieldOffset(2)]
	public byte ActionKind;

	[FieldOffset(3)]
	public byte DisplayDelay;

	[FieldOffset(4)]
	public uint RealActionId;

	[FieldOffset(8)]
	public float CastTime;

	[FieldOffset(12)]
	public uint TargetId;

	[FieldOffset(16)]
	public ushort _Facing;

	[FieldOffset(18)]
	public byte CanInterrupt;

	[FieldOffset(24)]
	public ushort PosX;

	[FieldOffset(26)]
	public ushort PosY;

	[FieldOffset(28)]
	public ushort PosZ;

	public readonly float Facing => (float)((double)(int)_Facing * 9.587526218325454E-05 - Math.PI);

	public readonly Vector3 Pos
	{
		get
		{
			float x = (float)(int)PosX * 0.030518044f - 1000f;
			float y = (float)(int)PosY * 0.030518044f - 1000f;
			float z = (float)(int)PosZ * 0.030518044f - 1000f;
			return new Vector3(x, y, z);
		}
	}
}

using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Struct.Vfx;

namespace Replica.Engine.Vfx;

public unsafe abstract class BaseVfx(string path)
{
	public unsafe VfxStruct* Vfx = null;

	public nint VfxHandle = IntPtr.Zero;

	public string Path = path;

	public abstract void Remove();

	public unsafe void Update()
	{
		if (Vfx != null)
		{
			byte* num = &Vfx->Flags;
			*num |= 2;
		}
	}

	public unsafe void UpdatePosition(Vector3 position)
	{
		if (Vfx != null)
		{
			Vfx->Position = new Vector3
			{
				X = position.X,
				Y = position.Y,
				Z = position.Z
			};
		}
	}

	public unsafe void UpdatePosition(IGameObject actor)
	{
		if (Vfx != null)
		{
			Vfx->Position = actor.Position;
		}
	}

	public unsafe void UpdateScale(Vector3 scale)
	{
		if (Vfx != null)
		{
			Vfx->Scale = new Vector3
			{
				X = scale.X,
				Y = scale.Y,
				Z = scale.Z
			};
		}
	}

	public unsafe void UpdateRotation(Vector3 rotation)
	{
		if (Vfx != null)
		{
			Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
			Vfx->Rotation = new Quat
			{
				X = quaternion.X,
				Y = quaternion.Y,
				Z = quaternion.Z,
				W = quaternion.W
			};
		}
	}

	public unsafe void UpdateColor(Vector4 Color)
	{
		if (Vfx != null)
		{
			Vfx->Color = Color;
		}
	}
}

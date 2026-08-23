using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace Replica.Engine.Helper;

public static class PositionHelper
{
	public unsafe static System.Numerics.Vector3 RenderPosition(this IGameObject obj)
	{
		if (obj.Address == IntPtr.Zero)
		{
			return obj.Position;
		}
		GameObject* address = (GameObject*)obj.Address;
		DrawObject* drawObject = address->DrawObject;
		if (drawObject != null)
		{
			return drawObject->Position;
		}
		return obj.Position;
	}

	public unsafe static bool StableWorldToScreen(System.Numerics.Vector3 world, out System.Numerics.Vector2 screen)
	{
		screen = default(System.Numerics.Vector2);
		FFXIVClientStructs.FFXIV.Client.Game.Camera* activeCamera = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager.Instance()->GetActiveCamera();
		FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera* ptr = ((activeCamera != null) ? activeCamera->SceneCamera.RenderCamera : null);
		if (ptr == null)
		{
			return Plugin.GameGui.WorldToScreen(world, out screen);
		}
		Device* ptr2 = Device.Instance();
		if (ptr2 == null)
		{
			return Plugin.GameGui.WorldToScreen(world, out screen);
		}
		FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4 viewMatrix = ptr->ViewMatrix;
		viewMatrix.M44 = 1f;
		FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4 matrix4x = viewMatrix * ptr->ProjectionMatrix;
		System.Numerics.Vector4 vector = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(world, 1f), matrix4x);
		if (vector.W <= 0f || Math.Abs(vector.W) < float.Epsilon)
		{
			return false;
		}
		float num = 1f / vector.W;
		System.Numerics.Vector2 pos = ImGuiHelpers.MainViewport.Pos;
		screen = new System.Numerics.Vector2(0.5f * (float)ptr2->Width * (1f + vector.X * num), 0.5f * (float)ptr2->Height * (1f - vector.Y * num)) + pos;
		return true;
	}

	public static bool AlmostEqual(this System.Numerics.Vector2 pos, System.Numerics.Vector2 pos2, float eps)
	{
		return (pos - pos2).AlmostZero(eps);
	}

	public static bool AlmostEqual(this System.Numerics.Vector3 pos, System.Numerics.Vector3 pos2, float eps)
	{
		return (pos - pos2).AlmostZero(eps);
	}

	public static bool AlmostZero(this System.Numerics.Vector2 a, float eps)
	{
		if (Math.Abs(a.X) <= eps)
		{
			return Math.Abs(a.Y) <= eps;
		}
		return false;
	}

	public static bool AlmostZero(this System.Numerics.Vector3 a, float eps)
	{
		if (Math.Abs(a.X) <= eps)
		{
			return Math.Abs(a.Z) <= eps;
		}
		return false;
	}

	public static System.Numerics.Vector2 RotationDegress(this System.Numerics.Vector2 offset, float degrees, bool clockwise = false)
	{
		float x = degrees * ((float)Math.PI / 180f);
		if (clockwise)
		{
			return new System.Numerics.Vector2(offset.X * MathF.Cos(x) - offset.Y * MathF.Sin(x), offset.X * MathF.Sin(x) + offset.Y * MathF.Cos(x));
		}
		return new System.Numerics.Vector2(offset.X * MathF.Cos(x) + offset.Y * MathF.Sin(x), (0f - offset.X) * MathF.Sin(x) + offset.Y * MathF.Cos(x));
	}

	public static System.Numerics.Vector2 ToVector2(this System.Numerics.Vector3 pos)
	{
		return new System.Numerics.Vector2(pos.X, pos.Z);
	}

	public static bool IsPointInsideField(System.Numerics.Vector3 center, System.Numerics.Vector3 point, float rotation = 0f, float halfSide = 10f)
	{
		double num = Math.Cos(0f - rotation);
		double num2 = Math.Sin(0f - rotation);
		double num3 = (double)(point.X - center.X) * num - (double)(point.Z - center.Z) * num2 + (double)center.X;
		double num4 = (double)(point.X - center.X) * num2 + (double)(point.Z - center.Z) * num + (double)center.Z;
		if (num3 >= (double)(center.X - halfSide) && num3 <= (double)(center.X + halfSide) && num4 >= (double)(center.Z - halfSide))
		{
			return num4 <= (double)(center.Z + halfSide);
		}
		return false;
	}
}

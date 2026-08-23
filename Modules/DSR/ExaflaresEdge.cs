using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class ExaflaresEdge : ISpecialAction
{
	public override string Name => "Exaflare's Edge";

	public override uint Phase => 7u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 28060u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null)
		{
			Vector3 position = gameObject.Position;
			Angle facing = info.Facing;
			for (int i = 0; i < 5; i++)
			{
				Vector3 vector = position - RotateVector(new Vector3(0f, 0f, -7 * (i + 1)), facing.Rad);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(vector.X, 0f, vector.Z),
					drawOnObject = false,
					radiusX = 6f,
					radiusZ = 6f,
					delayDrawTime = 6900 + i * 1900,
					destroyTime = 1900f
				}, Svc.Objects.LocalPlayer);
				Vector3 vector2 = position - RotateVector(new Vector3(-7 * (i + 1), 0f, 0f), facing.Rad);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(vector2.X, 0f, vector2.Z),
					drawOnObject = false,
					radiusX = 6f,
					radiusZ = 6f,
					delayDrawTime = 6900 + i * 1900,
					destroyTime = 1900f
				}, Svc.Objects.LocalPlayer);
				Vector3 vector3 = position - RotateVector(new Vector3(7 * (i + 1), 0f, 0f), facing.Rad);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(vector3.X, 0f, vector3.Z),
					drawOnObject = false,
					radiusX = 6f,
					radiusZ = 6f,
					delayDrawTime = 6900 + i * 1900,
					destroyTime = 1900f
				}, Svc.Objects.LocalPlayer);
			}
		}
	}

	private static Vector3 RotateVector(Vector3 vector, float rotation)
	{
		float num = MathF.Sin(rotation);
		float num2 = MathF.Cos(rotation);
		return new Vector3(vector.X * num2 + vector.Z * num, z: (0f - vector.X) * num + vector.Z * num2, y: vector.Y);
	}
}

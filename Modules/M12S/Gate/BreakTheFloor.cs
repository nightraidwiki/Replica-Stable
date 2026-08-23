using System;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Gate;

public class BreakTheFloor : ISpecialAction
{
	public override string Name => "Break the Floor";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46241u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((double)info.SourceId.GameObject().Position.X == 82.5 && (double)info.SourceId.GameObject().Position.Z == 87.5)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(100f, 0f, 85f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(85f, 0f, 95f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(100f, 0f, 105f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(115f, 0f, 95f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
		}
		else if (Math.Round(info.Pos.X) == 82.5 && Math.Round(info.Pos.Z) == 92.5)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(85f, 0f, 85f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(85f, 0f, 105f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(115f, 0f, 105f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "z6r1_b4_ibox_01k1",
				drawType = ElementType.Omen,
				Position = new Vector3(115f, 0f, 85f),
				drawOnObject = false,
				radiusZ = 10f,
				radiusX = 5f,
				destroyTime = 9700f,
				refColor = new Vector4(0f, 1f, 5f, 1f)
			});
		}
	}
}

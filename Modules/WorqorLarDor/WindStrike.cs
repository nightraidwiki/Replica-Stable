using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.WorqorLarDor;

public class WindStrike : ISpecialAction
{
	private static readonly Vector2 Pos1 = new Vector2(-43f, -57f);

	private static readonly Vector2 Pos2 = new Vector2(-63f, -57f);

	private static readonly Vector2 Pos3 = new Vector2(-53f, -47f);

	private static readonly Vector2 Pos4 = new Vector2(-53f, -67f);

	private static readonly DrawElement Circle = new DrawElement
	{
		drawAvfx = "customCircle",
		drawOnObject = false,
		radiusX = 15f,
		radiusZ = 15f,
		destroyTime = 5900f,
		refColor = GroundOmen.Yellow,
		refTargetColor = GroundOmen.Yellow
	};

	private static readonly DrawElement Donut = new DrawElement
	{
		drawAvfx = "customDonut",
		drawOnObject = false,
		refRadian = 0.16f,
		radiusX = 50f,
		radiusZ = 50f,
		destroyTime = 5900f
	};

	private static readonly DrawElement Rectangle = new DrawElement
	{
		drawAvfx = "customRect2",
		drawOnObject = false,
		fixRotation = true,
		radiusX = 7f,
		radiusZ = 50f,
		destroyTime = 5900f,
		refColor = GroundOmen.Yellow,
		refTargetColor = GroundOmen.Yellow
	};

	public override string Name => "Wind Strike";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnEnvControl(byte index, uint state)
	{
		if (state == 2097168 || state == 8388672)
		{
			DrawElement shape = ((state == 8388672) ? Donut : Circle);
			AddAOE(index, state, shape);
		}
	}

	private static void AddAOE(byte index, uint state, DrawElement shape)
	{
		switch (index)
		{
		case 30:
			shape.Position = new Vector3(Pos1.X, 323f, Pos1.Y);
			DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
			break;
		case 31:
			shape.Position = new Vector3(Pos2.X, 323f, Pos2.Y);
			DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
			break;
		case 32:
			if (state == 2097168)
			{
				shape = Rectangle;
				shape.Position = new Vector3(Pos1.X, 323f, Pos1.Y);
				shape.refRotation = 30.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
				shape.Position = new Vector3(Pos3.X, 323f, Pos3.Y);
				shape.refRotation = 100.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
				shape.Position = new Vector3(Pos4.X, 323f, Pos4.Y);
				shape.refRotation = 120.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
			}
			break;
		case 33:
			if (state == 2097168)
			{
				shape = Rectangle;
				shape.Position = new Vector3(Pos2.X, 323f, Pos2.Y);
				shape.refRotation = 30.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
				shape.Position = new Vector3(Pos3.X, 323f, Pos3.Y);
				shape.refRotation = 120.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
				shape.Position = new Vector3(Pos4.X, 323f, Pos4.Y);
				shape.refRotation = 100.Degrees();
				DrawManager.Draw(shape, Svc.Objects.LocalPlayer);
			}
			break;
		}
	}
}

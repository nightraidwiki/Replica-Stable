using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Vanguard;

public class HighSpeedSpin : ISpecialAction
{
	private static readonly DrawElement Circle = new DrawElement
	{
		drawAvfx = "customCircle",
		Position = new Vector3(-100f, 7f, 207f),
		drawOnObject = false,
		radiusX = 17f,
		radiusZ = 17f,
		refColor = GroundOmen.Yellow,
		refTargetColor = GroundOmen.Yellow
	};

	private static readonly DrawElement Donut = new DrawElement
	{
		drawAvfx = "customDonut",
		Position = new Vector3(-100f, 7f, 207f),
		drawOnObject = false,
		refRadian = 11f / 28f,
		radiusX = 28f,
		radiusZ = 28f,
		refColor = GroundOmen.Yellow,
		refTargetColor = GroundOmen.Yellow
	};

	public override string Name => "High-speed Spin";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39140u, 39141u, 36559u, 36560u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 36560:
		case 39140:
		{
			DrawElement donut = Donut;
			donut.hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			};
			DrawManager.Draw(donut, Svc.Objects.LocalPlayer);
			break;
		}
		case 36559:
		case 39141:
		{
			DrawElement circle = Circle;
			circle.hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			};
			DrawManager.Draw(circle, Svc.Objects.LocalPlayer);
			break;
		}
		}
	}
}

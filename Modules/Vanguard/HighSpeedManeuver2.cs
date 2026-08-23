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

public class HighSpeedManeuver2 : ISpecialAction
{
	public override string Name => "High-speed Maneuver 2";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36563u, 36564u, 37184u, 37191u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 36563:
		case 37184:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				Position = new Vector3(-107f, 7f, 207f),
				drawOnObject = false,
				radiusX = 7f,
				radiusZ = ((info.ActionId == 36563) ? 10 : 20),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				},
				refColor = GroundOmen.Yellow,
				refTargetColor = GroundOmen.Yellow
			}, Svc.Objects.LocalPlayer);
			break;
		case 36564:
		case 37191:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				Position = new Vector3(-93f, 7f, 207f),
				drawOnObject = false,
				radiusX = 7f,
				radiusZ = ((info.ActionId == 36564) ? 10 : 20),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				},
				refColor = GroundOmen.Yellow,
				refTargetColor = GroundOmen.Yellow
			}, Svc.Objects.LocalPlayer);
			break;
		}
	}
}

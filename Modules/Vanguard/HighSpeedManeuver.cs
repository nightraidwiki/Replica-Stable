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

public class HighSpeedManeuver : ISpecialAction
{
	public override string Name => "High-speed Maneuver";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39140u, 39141u, 36559u, 36560u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customRect",
			Position = new Vector3(-100f, 7f, 207f),
			drawOnObject = false,
			radiusX = 3f,
			radiusZ = 14f,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			},
			refColor = GroundOmen.Yellow,
			refTargetColor = GroundOmen.Yellow
		}, Svc.Objects.LocalPlayer);
	}
}

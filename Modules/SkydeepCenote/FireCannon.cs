using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.SkydeepCenote;

public class FireCannon : ISpecialAction
{
	public override string Name => "Fire Cannon";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38660u, 38661u, 38662u, 38663u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customRect2",
			radiusX = 5f,
			radiusZ = 5f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			},
			refColor = GroundOmen.Yellow,
			refTargetColor = GroundOmen.Yellow
		}, info.SourceId.GameObject());
	}
}

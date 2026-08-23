using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Origenics;

public class LaserCannon : ISpecialAction
{
	public override string Name => "Laser Cannon";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36366u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "m0248_freeze_o0c",
			radiusX = 5f,
			radiusZ = 40f,
			drawOnObject = true,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 36366u }
			}
		}, info.SourceId.GameObject());
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Alexandria;

public class SplitDischarge : ISpecialAction
{
	public override string Name => "Split Discharge";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36330u, 36331u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 12.5f,
			radiusZ = 45f,
			refOffsetX = ((info.ActionId == 36330) ? (-17.5f) : 17.5f),
			drawOnObject = true,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, info.SourceId.GameObject());
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class HallowedWings : ISpecialAction
{
	public override string Name => "Hallowed Wings";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40227u, 40228u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 20f,
			radiusZ = 80f,
			refRotation = ((info.ActionId == 40227) ? 90.Degrees() : (-90.Degrees())),
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = ActionID
			}
		}, info.SourceId.GameObject());
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UnendingCoil;

public class TenstrikeTrio : ISpecialAction
{
	public override string Name => "Tenstrike Trio";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9964u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bpxf",
			radiusX = 4f,
			radiusZ = 4f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, info.TargetId.GameObject());
		SimpleLockon.Share6S(info.TargetId.GameObject());
	}
}

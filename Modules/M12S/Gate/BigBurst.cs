using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Gate;

public class BigBurst : ISpecialAction
{
	public override string Name => "Big Burst";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46239u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		if (actorID.GameObject().BaseId == 2015017 && p1 == 16 && p2 == 32)
		{
			SimpleElement.Circle(actorID.GameObject(), 12f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46239u }
			});
		}
	}
}

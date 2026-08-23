using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class QuadrupleSwipe : ISpecialAction
{
	public override string Name => "Quadruple Swipe";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37982u, 38016u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.ShareLockon2(info.TargetId.GameObject());
	}
}

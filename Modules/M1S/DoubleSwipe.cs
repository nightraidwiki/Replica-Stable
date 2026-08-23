using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class DoubleSwipe : ISpecialAction
{
	public override string Name => "Double Swipe";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37984u, 38018u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.ShareLockon(info.TargetId.GameObject());
	}
}

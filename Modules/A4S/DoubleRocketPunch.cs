using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.A4S;

public class DoubleRocketPunch : ISpecialAction
{
	public override string Name => "Double Rocket Punch";

	public override HashSet<uint> ActionID => new HashSet<uint> { 5966u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 3f, 3000f, 0f, info.ActionId);
	}
}

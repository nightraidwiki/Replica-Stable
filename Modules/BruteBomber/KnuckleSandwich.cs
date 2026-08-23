using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BruteBomber;

public class KnuckleSandwich : ISpecialAction
{
	public override string Name => "Knuckle Sandwich";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37845u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 6f, 3000f, 0f, info.ActionId);
	}
}

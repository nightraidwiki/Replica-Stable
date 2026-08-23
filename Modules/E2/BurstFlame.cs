using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.E2;

public class BurstFlame : ISpecialAction
{
	public override string Name => "Burst Flame";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19437u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 4f, 3000f, 0f, info.ActionId);
	}
}

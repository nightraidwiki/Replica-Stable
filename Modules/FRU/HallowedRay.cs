using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class HallowedRay : ISpecialAction
{
	public override string Name => "Hallowed Ray";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40237u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 4f);
	}
}

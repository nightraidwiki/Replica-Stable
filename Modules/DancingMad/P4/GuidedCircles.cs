using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P4;

public class GuidedCircles : ISpecialAction
{
	public override string Name => "Guided Circles";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47906u, 47909u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

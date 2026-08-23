using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class MeteorImpact : ISpecialAction
{
	public override string Name => "Meteor Impact";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35676u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 10f);
	}
}

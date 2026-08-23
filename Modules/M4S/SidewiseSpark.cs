using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class SidewiseSpark : ISpecialAction
{
	public override string Name => "Sidewise Spark";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38380u, 38381u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 60f, 180);
	}
}

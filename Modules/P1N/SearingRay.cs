using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1N;

public class SearingRay : ISpecialAction
{
	public override string Name => "Searing Ray";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30423u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

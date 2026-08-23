using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M12S.Gate;

public class RavenousReach : ISpecialAction
{
	public override string Name => "Ravenous Reach";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46237u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

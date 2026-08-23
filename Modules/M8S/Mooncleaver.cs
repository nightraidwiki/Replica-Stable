using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class Mooncleaver : ISpecialAction
{
	public override string Name => "Mooncleaver";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42086u, 42829u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

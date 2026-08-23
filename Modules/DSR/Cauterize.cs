using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class Cauterize : ISpecialAction
{
	public override string Name => "Cauterize";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27533u, 27534u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 48f, 10f);
	}
}

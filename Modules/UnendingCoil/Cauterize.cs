using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UnendingCoil;

public class Cauterize : ISpecialAction
{
	public override string Name => "Cauterize";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9931u, 9932u, 9933u, 9934u, 9935u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 52f, 10f);
	}
}

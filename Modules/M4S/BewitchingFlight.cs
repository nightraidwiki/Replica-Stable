using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class BewitchingFlight : ISpecialAction
{
	public override string Name => "Bewitching Flight";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38377u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 40f, 2.5f);
	}
}

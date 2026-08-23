using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.WickedThunder;

public class BewitchingFlight : ISpecialAction
{
	public override string Name => "Bewitching Flight";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37560u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

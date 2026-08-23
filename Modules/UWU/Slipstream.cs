using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class Slipstream : ISpecialAction
{
	public override string Name => "Slipstream";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11091u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 11.7f, 90);
	}
}

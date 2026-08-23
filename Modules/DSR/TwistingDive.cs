using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class TwistingDive : ISpecialAction
{
	public override string Name => "Twisting Dive";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27531u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 60f, 5f);
	}
}

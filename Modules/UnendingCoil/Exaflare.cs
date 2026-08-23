using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UnendingCoil;

public class Exaflare : ISpecialAction
{
	public override string Name => "Exaflare";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9968u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineCircle(info, 8f, 1500f, 6);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.E11;

public class BurningStrike : ISpecialAction
{
	public override string Name => "Burning Strike";

	public override HashSet<uint> ActionID => new HashSet<uint> { 22060u, 22062u, 22064u, 22083u, 22085u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 40f, 5f, 40f);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TheMesoTerminal;

public class FlayingFlail : ISpecialAction
{
	public override string Name => "Flaying Flail";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43592u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 5f);
	}
}

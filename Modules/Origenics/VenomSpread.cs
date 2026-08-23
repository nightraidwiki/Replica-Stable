using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Origenics;

public class VenomSpread : ISpecialAction
{
	public override string Name => "Venom Spread";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38518u, 38519u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 6f, info.CastTime * 1000f);
	}
}

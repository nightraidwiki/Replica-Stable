using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TenderValley;

public class HardCoalBomb : ISpecialAction
{
	public override string Name => "Hard Coal Bomb";

	public override HashSet<uint> ActionID => new HashSet<uint> { 36401u, 36402u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.SourceId, 10f, info.CastTime * 1000f);
	}
}

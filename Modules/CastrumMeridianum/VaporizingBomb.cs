using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CastrumMeridianum;

public class VaporizingBomb : ISpecialAction
{
	public override string Name => "Vaporizing Bomb";

	public override HashSet<uint> ActionID => new HashSet<uint> { 28779u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 20f);
	}
}

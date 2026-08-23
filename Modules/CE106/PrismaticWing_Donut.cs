using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE106;

public class PrismaticWing_Donut : ISpecialAction
{
	public override string Name => "PrismaticWing - Donut";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42767u, 42769u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 5f, 31f);
	}
}

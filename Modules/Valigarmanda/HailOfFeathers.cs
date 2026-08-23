using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class HailOfFeathers : ISpecialAction
{
	public override string Name => "Hail of Feathers";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36893u, 36894u, 36895u, 36896u, 36897u, 36898u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 18f, (info.CastTime - 4f) * 1000f);
	}
}

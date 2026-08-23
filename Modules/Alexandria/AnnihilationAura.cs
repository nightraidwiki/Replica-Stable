using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Alexandria;

public class AnnihilationAura : ISpecialAction
{
	public override string Name => "Annihilation Aura";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39616u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 6f, 40f);
	}
}

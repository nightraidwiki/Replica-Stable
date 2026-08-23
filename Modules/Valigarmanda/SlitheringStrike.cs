using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class SlitheringStrike : ISpecialAction
{
	public override string Name => "Slithering Strike";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36812u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 24f, 180);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class DarkFireMoat : ISpecialAction
{
	public override string Name => "Dark Fire (moat)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40781u };

	public override uint Phase => 4u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info);
	}
}

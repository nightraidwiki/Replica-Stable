using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SanDoriaArc2;

public class SculptorSHandSpin : ISpecialAction
{
	public override string Name => "Sculptor's Hand (spin)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44440u, 44439u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info);
	}
}

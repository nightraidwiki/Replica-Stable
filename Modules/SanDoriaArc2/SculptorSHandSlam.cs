using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SanDoriaArc2;

public class SculptorSHandSlam : ISpecialAction
{
	public override string Name => "Sculptor's Hand (slam)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44442u, 44441u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

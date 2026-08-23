using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SanDoriaArc2;

public class SculptorSHand : ISpecialAction
{
	public override string Name => "Sculptor's Hand";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44444u, 44443u, 44459u, 44460u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

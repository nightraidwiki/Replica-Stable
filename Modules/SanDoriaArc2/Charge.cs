using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SanDoriaArc2;

public class Charge : ISpecialAction
{
	public override string Name => "Charge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44295u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

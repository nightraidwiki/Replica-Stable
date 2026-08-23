using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class GlowerPower : ISpecialAction
{
	public override string Name => "Glower Power";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 43340u, 43358u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

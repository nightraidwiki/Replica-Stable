using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.E11;

public class LightFlame : ISpecialAction
{
	public override string Name => "Light Flame";

	public override HashSet<uint> ActionID => new HashSet<uint> { 22075u, 22076u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M3S;

public class ProximityKnockback : ISpecialAction
{
	public override string Name => "Diveboom (knockback)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37868u, 37869u, 37877u, 37878u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37868:
		case 37877:
			SimpleElement.Circle(info, 20f);
			break;
		case 37869:
		case 37878:
			SimpleElement.KnockBack(info, 60f);
			break;
		}
	}
}

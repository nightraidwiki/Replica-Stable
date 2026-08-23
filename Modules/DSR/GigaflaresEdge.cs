using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class GigaflaresEdge : ISpecialAction
{
	public override string Name => "Gigaflare's Edge";

	public override uint Phase => 7u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 28058u, 28114u, 28115u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 28058:
			SimpleElement.Circle(info, 20f);
			break;
		case 28114:
		case 28115:
			SimpleElement.Circle(info.SourceId, 20f, 4000f, (info.CastTime - 4f) * 1000f);
			break;
		}
	}
}

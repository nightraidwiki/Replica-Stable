using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class AxeSyctheKick : ISpecialAction
{
	public override string Name => "Axe / Scythe Kick";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40202u, 40203u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40202:
			SimpleElement.Circle(info, 16f);
			break;
		case 40203:
			SimpleElement.Donut(info, 4f, 20f);
			break;
		}
	}
}

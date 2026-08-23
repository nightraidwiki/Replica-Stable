using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Everkeep;

public class HalfCircuit : ISpecialAction
{
	public override string Name => "Half Circuit";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37791u, 37792u, 37793u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37791)
		{
			SimpleElement.Rectangle(info, 60f, 60f);
		}
		if (info.ActionId == 37792)
		{
			SimpleElement.Donut(info, 10f, 30f);
		}
		if (info.ActionId == 37793)
		{
			SimpleElement.Circle(info, 10f);
		}
	}
}

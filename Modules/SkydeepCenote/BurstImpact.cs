using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SkydeepCenote;

public class BurstImpact : ISpecialAction
{
	public override string Name => "Burst Impact";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38581u, 38582u, 38583u, 38584u, 36445u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 38581:
			SimpleElement.Rectangle(info, 42f, 4f);
			break;
		case 38582:
			SimpleElement.Rectangle(info, 49f, 4f);
			break;
		case 38583:
			SimpleElement.Rectangle(info, 35f, 4f);
			break;
		case 38584:
			SimpleElement.Rectangle(info, 36f, 4f);
			break;
		case 36445:
			SimpleElement.Circle(info, 35f);
			break;
		}
	}
}

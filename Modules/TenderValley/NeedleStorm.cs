using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TenderValley;

public class NeedleStorm : ISpecialAction
{
	public override string Name => "Needle Storm";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37388u, 37389u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37388:
			SimpleElement.Circle(info, 6f);
			break;
		case 37389:
			SimpleElement.Circle(info, 11f);
			break;
		}
	}
}

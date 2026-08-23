using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M3S;

public class LariatInOut : ISpecialAction
{
	public override string Name => "Double Lariat (donut)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37864u, 37865u, 37866u, 37867u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37864:
		case 37866:
			SimpleElement.Circle(info, 10f);
			break;
		case 37865:
		case 37867:
			SimpleElement.Donut(info, 10f, 60f);
			break;
		}
	}
}

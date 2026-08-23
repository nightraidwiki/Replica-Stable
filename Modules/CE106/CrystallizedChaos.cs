using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE106;

public class CrystallizedChaos : ISpecialAction
{
	public override string Name => "CrystallizedChaos";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42729u, 42730u, 42731u, 41759u, 41760u, 41761u, 42733u, 42734u, 42735u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 41759:
		case 42729:
		case 42733:
			SimpleElement.Donut(info, 7f, 13f);
			break;
		case 41760:
		case 42730:
		case 42734:
			SimpleElement.Donut(info, 13f, 19f);
			break;
		case 41761:
		case 42731:
		case 42735:
			SimpleElement.Donut(info, 19f, 25f);
			break;
		}
	}
}

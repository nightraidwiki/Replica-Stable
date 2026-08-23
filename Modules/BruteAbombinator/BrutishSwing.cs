using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BruteAbombinator;

public class BrutishSwing : ISpecialAction
{
	public override string Name => "Brutish Swing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42271u, 42270u, 42293u, 42295u, 42302u, 42303u, 42317u, 42319u };

	public override void OnActionCast(ActorCastInfo info)
	{
		bool flag;
		switch (info.ActionId)
		{
		case 42271:
		case 42295:
		case 42303:
		case 42319:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			SimpleElement.Donut(info);
		}
		else
		{
			SimpleElement.Circle(info);
		}
	}
}

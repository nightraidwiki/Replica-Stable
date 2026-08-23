using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class VisceralWhirl : ISpecialAction
{
	public override string Name => "Visceral Whirl";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35652u, 35653u, 35655u, 35656u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 35652:
		case 35655:
			SimpleElement.Rectangle(info);
			break;
		case 35653:
			SimpleElement.Rectangle(info, 60f, 14f, 0f, new Vector2(-14f, 0f));
			break;
		case 35656:
			SimpleElement.Rectangle(info, 60f, 14f, 0f, new Vector2(14f, 0f));
			break;
		case 35654:
			break;
		}
	}
}

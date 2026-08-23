using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7;

public class LashingLariat : ISpecialAction
{
	public override string Name => "LashingLariat";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42322u, 42324u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 42322)
		{
			SimpleElement.Rectangle(info, 70f, 16f, 0f, new Vector2(8.2f, 0f));
		}
		else
		{
			SimpleElement.Rectangle(info, 70f, 16f, 0f, new Vector2(-8.2f, 0f));
		}
	}
}

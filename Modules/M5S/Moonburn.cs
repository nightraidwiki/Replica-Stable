using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M5S;

public class Moonburn : ISpecialAction
{
	public override string Name => "Moonburn";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42868u, 42867u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 42867)
		{
			SimpleElement.Rectangle(info, 40f, 7.5f, 0f, new Vector2(7.5f, 0f));
		}
		else
		{
			SimpleElement.Rectangle(info, 40f, 7.5f, 0f, new Vector2(-7.5f, 0f));
		}
	}
}

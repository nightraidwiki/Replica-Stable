using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class ItCameFromTheDirt : ISpecialAction
{
	public override string Name => "It Came From the Dirt";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42362u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 6f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { info.ActionId }
		});
	}
}

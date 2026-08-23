using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingGreen;

public class Moonburn : ISpecialAction
{
	public override string Name => "Moonburn";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42783u, 42784u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info.Pos, 40f, 7.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

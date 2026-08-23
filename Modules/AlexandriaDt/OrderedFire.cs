using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class OrderedFire : ISpecialAction
{
	public override string Name => "Ordered Fire";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42573u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info.Pos, 55f, 4f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

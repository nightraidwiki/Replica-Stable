using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class ConcurrentField : ISpecialAction
{
	public override string Name => "Concurrent Field";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42521u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.Pos, 26f, 50, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.A4S;

public class Hypercharge : ISpecialAction
{
	public override string Name => "Hypercharge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 5961u };

	public override void OnActionCast(ActorCastInfo info)
	{
		uint sourceId = info.SourceId;
		Angle facing = info.Facing;
		SimpleElement.Rectangle(sourceId, 70f, 2.5f, 0f, null, facing, 5000f);
	}
}

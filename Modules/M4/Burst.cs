using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4;

public class Burst : ISpecialAction
{
	public override string Name => "Burst (lightning lines)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37561u };

	public override void OnActionCast(ActorCastInfo info)
	{
		uint sourceId = info.SourceId;
		Angle facing = info.Facing;
		float castTime = info.CastTime * 1000f;
		SimpleElement.Rectangle(sourceId, 40f, 8f, 0f, null, facing, castTime);
	}
}

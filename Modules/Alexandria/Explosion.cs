using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Alexandria;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39239u };

	public override void OnActionCast(ActorCastInfo info)
	{
		uint sourceId = info.SourceId;
		Angle facing = info.Facing;
		float castTime = info.CastTime * 1000f;
		SimpleElement.Rectangle(sourceId, 50f, 4f, 50f, null, facing, castTime);
	}
}

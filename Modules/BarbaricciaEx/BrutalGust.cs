using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BarbaricciaEx;

public class BrutalGust : ISpecialAction
{
	public override string Name => "Brutal Gust";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30085u };

	public override void OnActionCast(ActorCastInfo info)
	{
		uint sourceId = info.SourceId;
		Angle facing = info.Facing;
		float castTime = info.CastTime * 1000f;
		SimpleElement.Rectangle(sourceId, 40f, 2f, 0f, null, facing, castTime);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Vanguard;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36575u, 36591u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 36575)
		{
			uint sourceId = info.SourceId;
			Angle facing = info.Facing;
			float castTime = info.CastTime * 1000f;
			SimpleElement.Rectangle(sourceId, 20f, 20f, 0f, null, facing, castTime);
		}
		if (info.ActionId == 36591)
		{
			uint sourceId2 = info.SourceId;
			Angle facing2 = info.Facing;
			float castTime2 = (info.CastTime - 4.5f) * 1000f;
			SimpleElement.Rectangle(sourceId2, 20f, 20f, 0f, null, facing2, castTime2, 4500f);
		}
	}
}

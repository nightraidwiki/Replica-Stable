using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CE107;

public class HeatedOutburst : ISpecialAction
{
	public override string Name => "HeatedOutburst";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37804u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 13f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 37804u },
			HitTarget = info.SourceId.GameObject()
		});
	}
}

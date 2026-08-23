using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CE105;

public class VerticalCrosshatch : ISpecialAction
{
	public override string Name => "VerticalCrosshatch";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41323u, 41330u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 0f.Degrees(), info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 180f.Degrees(), info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 90f.Degrees(), 2000f, info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, -90f.Degrees(), 2000f, info.CastTime * 1000f);
	}
}

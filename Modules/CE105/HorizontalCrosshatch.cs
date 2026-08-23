using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CE105;

public class HorizontalCrosshatch : ISpecialAction
{
	public override string Name => "HorizontalCrosshatch";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41324u, 41331u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 90f.Degrees(), info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, -90f.Degrees(), info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 0f.Degrees(), 2000f, info.CastTime * 1000f);
		SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 180f.Degrees(), 2000f, info.CastTime * 1000f);
	}
}

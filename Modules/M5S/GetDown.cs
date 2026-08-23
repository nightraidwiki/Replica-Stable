using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M5S;

public class GetDown : ISpecialAction
{
	public override string Name => "GetDown! Bait";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42853u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.SourceId, 40f, 45, info.Facing, info.CastTime * 1000f);
	}
}

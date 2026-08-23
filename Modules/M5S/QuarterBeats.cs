using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M5S;

public class QuarterBeats : ISpecialAction
{
	public override string Name => "Quarter Beats";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42844u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.ShareLockon2(info.TargetId.GameObject());
	}
}

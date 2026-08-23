using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BruteAbombinator;

public class QuarrySwamp : ISpecialAction
{
	public override string Name => "Quarry Swamp";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42285u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.EyeWarn(info.SourceId.GameObject());
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class SinboundMeltdownStack : ISpecialAction
{
	public override string Name => "Sinbound Meltdown (stack)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40286u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.ShareLockon(info.SourceId.GameObject());
	}
}

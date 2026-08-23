using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M5S;

public class EighthBeats : ISpecialAction
{
	public override string Name => "Eighth Beats";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42846u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.TarLockOn5m5s(info.TargetId.GameObject());
	}
}

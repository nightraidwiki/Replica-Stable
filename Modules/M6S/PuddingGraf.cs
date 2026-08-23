using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class PuddingGraf : ISpecialAction
{
	public override string Name => "Pudding Graf";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42678u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleLockon.TarLockOn6m5s(info.TargetId.GameObject());
	}
}

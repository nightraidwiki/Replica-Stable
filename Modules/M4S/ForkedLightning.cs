using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class ForkedLightning : ISpecialAction
{
	public override string Name => "Forked Lightning (buff)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 587)
		{
			SimpleLockon.TarLockOn5m8s(info.TargetID.GameObject());
		}
	}
}

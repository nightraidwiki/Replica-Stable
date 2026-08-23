using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class DarkSpread : ISpecialAction
{
	public override string Name => "Dark (spread)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2460)
		{
			SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
		}
	}
}

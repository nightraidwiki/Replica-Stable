using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Gate;

public class DetonateCell : ISpecialAction
{
	public override string Name => "Detonate Cell";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4761)
		{
			SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject(), info.Time * 1000f - 5000f);
		}
	}
}

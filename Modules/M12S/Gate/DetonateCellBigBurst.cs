using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Gate;

public class DetonateCellBigBurst : ISpecialAction
{
	public override string Name => "Detonate Cell (Big Burst)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4762)
		{
			SimpleLockon.Share6S(info.TargetID.GameObject(), info.Time * 1000f - 6000f);
		}
	}
}

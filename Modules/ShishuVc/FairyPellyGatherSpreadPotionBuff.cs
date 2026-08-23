using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class FairyPellyGatherSpreadPotionBuff : ISpecialAction
{
	public override string Name => "Fairy Pelly Gather/Spread Potion (buff)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4615)
		{
			SimpleElement.Circle(info.TargetID, 15f, 5000f, (info.Time - 5f) * 1000f);
		}
		if (info.StatusID == 4616)
		{
			SimpleLockon.ShareLockon2(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
		}
	}
}

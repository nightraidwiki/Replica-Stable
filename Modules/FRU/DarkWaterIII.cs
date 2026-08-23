using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class DarkWaterIII : ISpecialAction
{
	public override string Name => "Dark Water III (stack)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2461)
		{
			SimpleLockon.ShareLockon(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
			new TimeHelper((long)((info.Time - 5f) * 1000f), delegate
			{
				SimpleElement.ShowText("Real stack");
			});
		}
	}
}

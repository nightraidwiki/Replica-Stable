using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class ReturnSavedPosition : ISpecialAction
{
	public override string Name => "Return (saved position)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4208)
		{
			new TimeHelper((long)((info.Time - 5f) * 1000f), delegate
			{
				SimpleElement.ShowText("Note your position");
			});
		}
	}
}

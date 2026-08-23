using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuDeep;

public class FairyPellySpreadPotionBuff : ISpecialAction
{
	public override string Name => "Fairy Pelly Spread Potion (buff)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4615)
		{
			SimpleElement.Circle(info.TargetID, 15f, 5000f, (info.Time - 5f) * 1000f);
		}
	}
}

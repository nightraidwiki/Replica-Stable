using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class BlightedBolt : ISpecialAction
{
	public override string Name => "Blighted Bolt";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36831u, 36833u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 36831)
		{
			SimpleElement.ShowText("Don't float", RaptureAtkModule.TextGimmickHintStyle.Info);
		}
		if (info.ActionId == 36833)
		{
			SimpleElement.Circle(info.TargetId, 8f, 3000f, 0f, 36833u);
		}
	}
}

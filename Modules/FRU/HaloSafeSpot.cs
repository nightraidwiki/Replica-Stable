using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class HaloSafeSpot : ISpecialAction
{
	public override string Name => "Halo (safe spot)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40150u, 40151u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40150:
			SimpleElement.ShowText("Thunder safe", RaptureAtkModule.TextGimmickHintStyle.Info);
			break;
		case 40151:
			SimpleElement.ShowText("Fire safe", RaptureAtkModule.TextGimmickHintStyle.Info);
			break;
		}
	}
}

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Interop;

namespace Replica.Modules.Enuo;

public class Meltdown : ISpecialAction
{
	public override string Name => "Meltdown";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		50041u, // Meltdown1 (puddle)
		50042u  // Meltdown2 (spread)
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 50041u)
		{
			SimpleElement.Circle(info, 5f);
		}
		else if (info.ActionId == 50042u)
		{
			SimpleElement.Circle(info.TargetId, 5f, info.CastTime * 1000f, 0f, info.ActionId);
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4562u && Svc.Objects.LocalPlayer != null && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			SimpleElement.ShowText("Don't move", RaptureAtkModule.TextGimmickHintStyle.Warning, 5);
		}
	}

	public override void OnRemoveStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4562u && Svc.Objects.LocalPlayer != null && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			SimpleElement.ShowText("Move!", RaptureAtkModule.TextGimmickHintStyle.Info, 4);
		}
	}
}

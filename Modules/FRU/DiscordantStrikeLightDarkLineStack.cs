using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class DiscordantStrikeLightDarkLineStack : ISpecialAction
{
	public override string Name => "Discordant Strike (light/dark line stack)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		uint statusID = info.StatusID;
		if ((statusID == 3323 || statusID == 4164) && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			SimpleElement.ShowText("Swap sides!", RaptureAtkModule.TextGimmickHintStyle.Warning, 3);
		}
	}
}

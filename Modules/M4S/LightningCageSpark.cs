using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class LightningCageSpark : ISpecialAction
{
	public override string Name => "Ion Cluster (debuff)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 3999 || info.TargetID != Svc.Objects.LocalPlayer?.GameObjectId)
		{
			return;
		}
		float time = info.Time;
		if (time != 22f)
		{
			if (time == 42f)
			{
				SimpleElement.ShowText("Long debuff", RaptureAtkModule.TextGimmickHintStyle.Info);
			}
		}
		else
		{
			SimpleElement.ShowText("Short debuff", RaptureAtkModule.TextGimmickHintStyle.Info);
		}
		new TimeHelper((long)((info.Time - 7f) * 1000f), delegate
		{
			int num = WitchGleam.Players.FindIndex((IGameObject x) => x.GameObjectId == info.TargetID);
			switch (WitchGleam.Stacks[num] switch
			{
				1 => 12, 
				2 => (info.Time > 30f) ? 20 : 12, 
				3 => 20, 
				_ => 0, 
			})
			{
			case 12:
				SimpleElement.ShowText("Small thunder — go inside", RaptureAtkModule.TextGimmickHintStyle.Info, 7);
				break;
			case 20:
				SimpleElement.ShowText("Big thunder — go corner", RaptureAtkModule.TextGimmickHintStyle.Warning, 7);
				break;
			}
		});
	}
}

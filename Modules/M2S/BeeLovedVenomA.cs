using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M2S;

public class BeeLovedVenomA : ISpecialAction
{
	public override string Name => "Bee-loved Venom α (tether)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 3932)
		{
			return;
		}
		if (info.Time == 12f)
		{
			if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
			{
				SimpleElement.ShowText("Poison 1");
			}
			SimpleLockon.Dice1_5s(info.TargetID.GameObject());
		}
		else if (info.Time == 28f)
		{
			if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
			{
				SimpleElement.ShowText("Poison 2");
			}
			SimpleLockon.Dice2_5s(info.TargetID.GameObject());
		}
		else if (info.Time == 44f)
		{
			if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
			{
				SimpleElement.ShowText("Poison 3");
			}
			SimpleLockon.Dice3_5s(info.TargetID.GameObject());
		}
		else if (info.Time == 62f)
		{
			if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
			{
				SimpleElement.ShowText("Poison 4");
			}
			SimpleLockon.Dice4_5s(info.TargetID.GameObject());
		}
		new TimeHelper((long)(info.Time - 8f) * 1000, delegate
		{
			IPlayerCharacter playerCharacter = (from player in PlayerHelper.AllPlayers.OfType<IPlayerCharacter>()
				where player.StatusList.Any((IStatus status) => status.StatusId == 3933)
				orderby player.StatusList.First((IStatus status) => status.StatusId == 3933).RemainingTime
				select player).FirstOrDefault();
			if (playerCharacter != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawType = ElementType.Channeling,
					drawAvfx = "chan_dna_recombinant_ok0k1",
					drawOnObject = true,
					target = playerCharacter,
					delayDrawTime = (info.Time - 8f) * 1000f,
					destroyTime = 8000f,
					StatusCheck = new StatusCheck
					{
						CheckObject = info.TargetID.GameObject(),
						Status = 3932u
					}
				}, info.TargetID.GameObject(), playerCharacter);
			}
		});
	}
}

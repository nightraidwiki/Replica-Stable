using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M2S;

public class BeeLovedVenomB : ISpecialAction
{
	public override string Name => "Bee-loved Venom β (buff)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 3933)
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
	}
}

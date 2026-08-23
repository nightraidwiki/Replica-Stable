using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M3S;

public class Bombarium : ISpecialAction
{
	public override string Name => "Bombarium";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 4020)
		{
			return;
		}
		float time = info.Time;
		if (time != 26f)
		{
			if (time == 44f)
			{
				if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
				{
					SimpleElement.ShowText("Long debuff");
				}
				SimpleLockon.Dice2_5s(info.TargetID.GameObject());
			}
		}
		else
		{
			if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Short debuff");
			}
			SimpleLockon.Dice1_5s(info.TargetID.GameObject());
		}
	}
}

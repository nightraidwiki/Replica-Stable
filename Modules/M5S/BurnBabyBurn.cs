using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M5S;

public class BurnBabyBurn : ISpecialAction
{
	public override string Name => "BurnBabyBurn";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4461 && info.TargetID == Svc.Objects.LocalPlayer?.GameObjectId)
		{
			float time = info.Time;
			if (time == 9.5f || time == 23.5f)
			{
				SimpleLockon.Dice1_5s(Svc.Objects.LocalPlayer);
			}
			else
			{
				SimpleLockon.Dice2_5s(Svc.Objects.LocalPlayer);
			}
		}
	}
}

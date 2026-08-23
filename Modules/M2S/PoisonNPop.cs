using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M2S;

public class PoisonNPop : ISpecialAction
{
	public override string Name => "Poison N Pop (buff)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 3934)
		{
			return;
		}
		if (info.Time == 26f)
		{
			if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Short debuff");
			}
		}
		else if (info.Time == 46f && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			SimpleElement.ShowText("Long debuff");
		}
		SimpleElement.Circle(info.TargetID.GameObject(), 14f, 4000f, (info.Time - 4f) * 1000f);
	}
}

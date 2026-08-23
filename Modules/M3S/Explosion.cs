using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M3S;

public class Explosion : ISpecialAction
{
	public override string Name => "Fuses of Fury (buff)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		switch (info.StatusID)
		{
		case 4024u:
			if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Short line");
			}
			break;
		case 4025u:
			if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Long line");
			}
			break;
		case 4026u:
			SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject());
			break;
		case 4027u:
			SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject(), 5000f);
			break;
		}
	}
}

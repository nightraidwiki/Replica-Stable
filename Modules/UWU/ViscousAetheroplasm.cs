using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UWU;

public class ViscousAetheroplasm : ISpecialAction
{
	public override string Name => "Viscous Aetheroplasm";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 1532)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "com_share0c",
				delayDrawTime = (info.Time - 5f) * 1000f
			}, info.TargetID.GameObject());
		}
	}
}

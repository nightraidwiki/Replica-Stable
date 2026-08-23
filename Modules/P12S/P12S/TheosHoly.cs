using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class TheosHoly : ISpecialAction
{
	public override string Name => "Heavensflame Sigil (buff)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3578)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "loc06sp_05ak1",
				delayDrawTime = (info.Time - 5f) * 1000f
			}, info.TargetID.GameObject());
		}
	}
}

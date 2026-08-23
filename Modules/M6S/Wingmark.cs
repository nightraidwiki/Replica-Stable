using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M6S;

public class Wingmark : ISpecialAction
{
	public override string Name => "Wingmark";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4450 && info.TargetID == Svc.Objects.LocalPlayer?.GameObjectId)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 34f,
				drawOnObject = true,
				destroyTime = info.Time * 1000f,
				StatusCheck = new StatusCheck
				{
					CheckObject = Svc.Objects.LocalPlayer,
					Status = 4450u
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}

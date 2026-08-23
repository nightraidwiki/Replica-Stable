using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.QueenEternalEx;

public class WindSigilKnockback : ISpecialAction
{
	public override string Name => "Wind Sigil (knockback)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID - 4189 <= 1 && info.TargetID == Svc.Objects.LocalPlayer?.GameObjectId)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 20f,
				refRotation = ((info.StatusID == 4189) ? (-90.Degrees()) : 90.Degrees()),
				fixRotation = true,
				delayDrawTime = (info.Time - 5f) * 1000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40996u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}

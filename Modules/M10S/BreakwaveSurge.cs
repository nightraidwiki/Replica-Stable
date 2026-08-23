using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M10S;

public class BreakwaveSurge : ISpecialAction
{
	public override string Name => "Breakwave Surge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46540u, 46542u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 46542:
			SimpleElement.Rectangle(info);
			break;
		case 46540:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 10f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = ActionID
				},
				KnockBackCheck = new KnockBackCheck
				{
					Angle = info.Facing
				}
			}, Svc.Objects.LocalPlayer);
			break;
		}
	}
}

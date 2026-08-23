using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CloudOfDarkness;

public class P2KnockbackDisplacement : ISpecialAction
{
	public override string Name => "Pull/knockback displacement";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40515u, 40524u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.ShowText((info.ActionId == 40515) ? "Pull" : "Knockback");
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 15f,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = new Vector3(100f, 0f, 76.28425f),
				Reverse = (info.ActionId == 40515)
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40516u, 40522u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

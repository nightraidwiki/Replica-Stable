using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ForkedTower;

public class ChillingCollision : ISpecialAction
{
	public override string Name => "Chilling Collision";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42422u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 22f,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.Pos
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 42422u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

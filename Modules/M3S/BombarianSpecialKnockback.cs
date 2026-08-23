using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M3S;

public class BombarianSpecialKnockback : ISpecialAction
{
	public override string Name => "Bombarian Special (knockback)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37908u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 10f,
			drawOnObject = true,
			delayDrawTime = 20000f,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = new Vector3(100f, 0f, 100f)
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class AeroIII : ISpecialAction
{
	public override string Name => "AeroIII";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41912u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 8f,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.Pos
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

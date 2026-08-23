using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TheMesoTerminal;

public class PungentAerosol : ISpecialAction
{
	public override string Name => "Pungent Aerosol";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43807u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.KnockBack(info.Pos, 20f, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			drawOnObject = true,
			radiusX = 1f,
			radiusZ = 24f,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.Pos,
				Antiable = false
			},
			hitCounter = new HitCounter
			{
				ActionID = ActionID
			}
		}, Svc.Objects.LocalPlayer);
	}
}

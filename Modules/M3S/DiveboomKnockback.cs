using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M3S;

public class DiveboomKnockback : ISpecialAction
{
	public override string Name => "Diveboom (knockback)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37869u, 37878u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 25f,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.SourceId.GameObject().Position
			},
			hitCounter = new HitCounter
			{
				ActionID = ActionID
			}
		}, Svc.Objects.LocalPlayer);
	}
}

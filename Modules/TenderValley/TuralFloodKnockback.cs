using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TenderValley;

public class TuralFloodKnockback : ISpecialAction
{
	public override string Name => "Tural Flood (knockback)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 36756u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 15f,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.SourceId.GameObject().Position
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 36756u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

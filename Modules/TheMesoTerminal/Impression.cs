using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TheMesoTerminal;

public class Impression : ISpecialAction
{
	public override string Name => "Impression";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43819u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			drawOnObject = true,
			radiusX = 1f,
			radiusZ = 11f,
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

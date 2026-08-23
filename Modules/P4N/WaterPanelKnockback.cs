using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P4N;

public class WaterPanelKnockback : ISpecialAction
{
	public override string Name => "Water Panel (knockback)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 27198u };

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
				OriginPos = new Vector3(100f, 0f, 100f)
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 27194u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}

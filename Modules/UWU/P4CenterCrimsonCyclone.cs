using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UWU;

public class P4CenterCrimsonCyclone : ISpecialAction
{
	public override string Name => "P4 Center Crimson Cyclone";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11596u, 11103u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 11596)
		{
			base.CanDraw = true;
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 11103 && base.CanDraw)
		{
			base.CanDraw = false;
			ActionQueue.Enqueue((new HashSet<uint> { 11103u }, action));
		}
		static void action()
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "general_x02f",
				Position = new Vector3(100f, 0f, 100f),
				drawOnObject = false,
				radiusX = 5f,
				radiusZ = 20f,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 11104u }
				}
			};
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			obj.refRotation = 90.Degrees();
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
		}
	}
}

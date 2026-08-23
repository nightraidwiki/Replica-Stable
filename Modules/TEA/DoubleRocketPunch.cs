using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class DoubleRocketPunch : ISpecialAction
{
	public override string Name => "Double Rocket Punch";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18503u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 3f,
			radiusZ = 3f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18503u }
			}
		};
		if (Svc.Objects.LocalPlayer.GetRole() == CombatRole.Tank)
		{
			drawElement.drawAvfx = "general_1bpxf";
		}
		DrawManager.Draw(drawElement, info.TargetId.GameObject());
	}
}

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

public class OrdainedCapitalPunishment : ISpecialAction
{
	public override string Name => "Ordained Capital Punishment";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18578u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = ((Svc.Objects.LocalPlayer.GetRole() == CombatRole.Tank) ? "general_1bpxf" : "general_1bxf"),
			radiusX = 4f,
			radiusZ = 4f,
			alwaysDrawOnCurrentTarget = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18579u },
				TargetHitCount = 3
			}
		}, info.SourceId.GameObject());
	}
}

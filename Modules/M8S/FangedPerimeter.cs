using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class FangedPerimeter : ISpecialAction
{
	public override string Name => "Fanged Maw / Perimeter";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42083u, 42084u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 42083:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				radiusX = 22f,
				radiusZ = 22f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
			break;
		case 42084:
			SimpleElement.Donut(info);
			break;
		}
	}
}

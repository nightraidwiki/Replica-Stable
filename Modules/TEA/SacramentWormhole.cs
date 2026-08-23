using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class SacramentWormhole : ISpecialAction
{
	public override string Name => "Sacrament";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18519u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement obj = new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 8f,
			radiusZ = 100f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18519u }
			}
		};
		DrawManager.Draw(obj, info.SourceId.GameObject());
		obj.refRotation = 90.Degrees();
		DrawManager.Draw(obj, info.SourceId.GameObject());
	}
}

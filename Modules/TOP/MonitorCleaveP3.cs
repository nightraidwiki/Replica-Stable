using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class MonitorCleaveP3 : ISpecialAction
{
	public override string Name => "Monitor Cleave (P3)";

	public override uint Phase => 3u;

	public override uint WeatherID => 79u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31595u, 31596u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan180_1bf",
			radiusX = 100f,
			radiusZ = 100f,
			drawOnObject = true,
			refRotation = ((info.ActionId == 31595) ? 90.Degrees() : (-90.Degrees())),
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, info.SourceId.GameObject());
	}
}

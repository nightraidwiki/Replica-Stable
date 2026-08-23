using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Everkeep;

public class WindOfSpite : ISpecialAction
{
	public override string Name => "Wind of Spite";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39229u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 5f,
			radiusZ = 5f,
			drawOnObject = true,
			alwaysDrawOnCurrentTarget = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 39230u, 39232u },
				TargetHitCount = 3
			}
		}, info.SourceId.GameObject());
	}
}

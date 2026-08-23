using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class WickedJolt : ISpecialAction
{
	public override string Name => "Wicked Jolt";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38384u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 2.5f,
			radiusZ = 60f,
			drawOnObject = true,
			target = info.TargetId.GameObject(),
			alwaysFaceCurrentTarget = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38384u, 38385u },
				TargetHitCount = 2
			}
		}, info.SourceId.GameObject());
	}
}

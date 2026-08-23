using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class Overshadow : ISpecialAction
{
	public override string Name => "Overshadow";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38039u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02pxf",
			radiusX = 2.5f,
			radiusZ = 100f,
			drawOnObject = true,
			target = info.TargetId.GameObject(),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38040u }
			}
		}, info.SourceId.GameObject());
	}
}

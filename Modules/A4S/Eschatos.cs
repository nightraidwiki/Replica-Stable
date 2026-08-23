using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.A4S;

public class Eschatos : ISpecialAction
{
	public override string Name => "Eschatos";

	public override HashSet<uint> ActionID => new HashSet<uint> { 5969u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan090_1bf",
			radiusX = 25f,
			radiusZ = 25f,
			drawOnObject = true,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 5970u },
				TargetHitCount = 5
			}
		}, info.SourceId.GameObject());
	}
}

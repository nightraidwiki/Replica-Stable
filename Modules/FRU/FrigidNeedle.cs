using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class FrigidNeedle : ISpecialAction
{
	public override string Name => "Frigid Needle";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40201u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement obj = new DrawElement
		{
			drawAvfx = "customRect2",
			radiusX = 2.5f,
			radiusZ = 40f,
			drawOnObject = true,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = ActionID
			},
			refColor = new Vector4(1f, 1f, 1f, 0.2f),
			refTargetColor = new Vector4(1f, 1f, 1f, 0.2f)
		};
		DrawManager.Draw(obj, info.SourceId.GameObject());
		obj.refRotation += 90.Degrees();
		DrawManager.Draw(obj, info.SourceId.GameObject());
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class Towerfall : ISpecialAction
{
	public override string Name => "Towerfall";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41925u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement obj = new DrawElement
		{
			drawAvfx = "general02xf",
			Position = info.Pos,
			drawOnObject = false,
			radiusX = 5f,
			radiusZ = 30f,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 41926u }
			}
		};
		DrawManager.Draw(obj);
		obj.refRotation += 180.Degrees();
		DrawManager.Draw(obj);
	}
}

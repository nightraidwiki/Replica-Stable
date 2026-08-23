using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ForkedTower;

public class LineofFire : ISpecialAction
{
	public override string Name => "Line of Fire";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42965u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02xf",
			Position = info.Pos,
			drawOnObject = false,
			radiusX = 4f,
			radiusZ = 60f,
			refRotation = info.Facing,
			destroyTime = 9000f
		});
	}
}

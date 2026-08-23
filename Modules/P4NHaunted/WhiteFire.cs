using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.P4NHaunted;

public class WhiteFire : ISpecialAction
{
	public override string Name => "White Fire";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 17)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 2f,
				radiusZ = 100f,
				drawOnObject = true,
				target = targetId.GameObject(),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 33482u }
				}
			}, actorId.GameObject());
		}
	}
}

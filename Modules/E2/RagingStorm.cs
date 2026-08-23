using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.E2;

public class RagingStorm : ISpecialAction
{
	public override string Name => "Raging Storm";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 109)
		{
			SimpleElement.FanToTarget(targetId, actorId, 40f, 45, Follow: true, default(Angle), 3000f, 19425u);
		}
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.P4NHaunted;

public class SoulChain : ISpecialAction
{
	public override string Name => "Soul Chain";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 232)
		{
			SimpleElement.Rectangle(actorId, 5f, 10f, 5f, null, default(Angle), 3000f, 0f, 33472u);
		}
	}
}

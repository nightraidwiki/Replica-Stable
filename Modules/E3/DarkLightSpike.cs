using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.E3;

public class DarkLightSpike : ISpecialAction
{
	public override string Name => "Dark Light Spike";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 37)
		{
			SimpleElement.RectangleToTarget(actorId, targetId, 100f, 3f, 3000f, 19507u);
		}
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TEA;

public class Plasmasphere : ISpecialAction
{
	public override string Name => "Plasma Sphere";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 12)
		{
			SimpleElement.Circle(targetId, 6f, 16000f);
		}
	}
}

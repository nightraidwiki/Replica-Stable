using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class OrdainedPunishment : ISpecialAction
{
	public override string Name => "Ordained Punishment";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18577u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 5f, 3000f, 0f, 18577u);
	}
}

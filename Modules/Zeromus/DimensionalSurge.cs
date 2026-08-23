using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class DimensionalSurge : ISpecialAction
{
	public override string Name => "Dimensional Surge";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35714u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

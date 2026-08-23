using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Origenics;

public class SwingingArm : ISpecialAction
{
	public override string Name => "Swinging Arm";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36370u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 30f, 90);
	}
}

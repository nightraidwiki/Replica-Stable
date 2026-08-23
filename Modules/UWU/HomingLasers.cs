using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class HomingLasers : ISpecialAction
{
	public override string Name => "Homing Lasers";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11131u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 4f, 3000f, 0f, 11131u);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M11S;

public class BeastFlameTail : ISpecialAction
{
	public override string Name => "Beast Flame Tail";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46128u, 46129u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 90);
	}
}

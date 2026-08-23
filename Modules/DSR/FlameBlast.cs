using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class FlameBlast : ISpecialAction
{
	public override string Name => "Flame Blast";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 26409u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info.SourceId, 44f, 3f, info.Facing, 5000f);
	}
}

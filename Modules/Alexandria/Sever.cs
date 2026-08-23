using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Alexandria;

public class Sever : ISpecialAction
{
	public override string Name => "Sever";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39007u, 39238u, 39249u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 40f, 180);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class GleamingBeamBarrage : ISpecialAction
{
	public override string Name => "Gleaming Beam / Barrage";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42078u, 42102u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class HeroBlow : ISpecialAction
{
	public override string Name => "Hero Blow";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42080u, 42082u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

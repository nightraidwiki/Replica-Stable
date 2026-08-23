using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class DarkAuraCone : ISpecialAction
{
	public override string Name => "Dark Aura (cone)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40290u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.FanToTarget(info, 60f, 90);
	}
}

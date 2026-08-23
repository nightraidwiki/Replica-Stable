using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BruteBomber;

public class MurderousMist : ISpecialAction
{
	public override string Name => "Murderous Mist";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37813u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 60f, 270);
	}
}

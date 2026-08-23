using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UnendingCoil;

public class MegaflareDive : ISpecialAction
{
	public override string Name => "Megaflare Dive";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9953u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 60f, 6f);
	}
}

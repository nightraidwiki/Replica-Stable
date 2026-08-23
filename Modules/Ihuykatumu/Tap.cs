using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Ihuykatumu;

public class Tap : ISpecialAction
{
	public override string Name => "Tap";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36479u, 36482u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 36479)
		{
			SimpleElement.Rectangle(info, 40f, 5f);
		}
		if (info.ActionId == 36482)
		{
			SimpleElement.Rectangle(info, 40f, 8f);
		}
	}
}

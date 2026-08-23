using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Alexandria;

public class CurrentMatrix : ISpecialAction
{
	public override string Name => "Current Matrix";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39136u, 39138u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 39136)
		{
			SimpleElement.Triangle(info, 40f, 90);
		}
		if (info.ActionId == 39138)
		{
			SimpleElement.Rectangle(info, 55f, 4f);
		}
	}
}

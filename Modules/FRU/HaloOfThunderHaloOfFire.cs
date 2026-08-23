using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class HaloOfThunderHaloOfFire : ISpecialAction
{
	public override string Name => "Halo of Thunder / Halo of Fire";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40152u, 40153u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40152:
			SimpleElement.Circle(info, 5f);
			break;
		case 40153:
			SimpleElement.Circle(info, 10f);
			break;
		}
	}
}

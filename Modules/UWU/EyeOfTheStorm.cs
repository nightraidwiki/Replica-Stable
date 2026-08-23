using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class EyeOfTheStorm : ISpecialAction
{
	public override string Name => "Eye of the Storm";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11090u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 12f, 25f);
	}
}

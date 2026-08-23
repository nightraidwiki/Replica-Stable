using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE110;

public class ShadesNest : ISpecialAction
{
	public override string Name => "Shades' Nest";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42033u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 7f, 50f);
	}
}

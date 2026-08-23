using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1NHaunted;

public class Freeze : ISpecialAction
{
	public override string Name => "Freeze";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33057u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 8f, 30f);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class RootsOfEvil : ISpecialAction
{
	public override string Name => "Roots of Evil";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42354u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 12f);
	}
}

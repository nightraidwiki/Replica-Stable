using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class Pollen : ISpecialAction
{
	public override string Name => "Pollen";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42347u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 8f, 4000f);
	}
}

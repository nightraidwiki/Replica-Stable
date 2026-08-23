using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P3N;

public class LeafBladeOfAtis : ISpecialAction
{
	public override string Name => "Leaf Blade of Atis";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30725u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineCircle(info, 7f, 1300f, 8);
	}
}

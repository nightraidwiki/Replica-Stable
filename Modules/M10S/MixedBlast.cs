using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M10S;

public class MixedBlast : ISpecialAction
{
	public override string Name => "Mixed Blast";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46587u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

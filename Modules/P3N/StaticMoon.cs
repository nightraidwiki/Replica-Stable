using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P3N;

public class StaticMoon : ISpecialAction
{
	public override string Name => "Static Moon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30722u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

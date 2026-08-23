using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE107;

public class ScathingSweep : ISpecialAction
{
	public override string Name => "ScathingSweep";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42691u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

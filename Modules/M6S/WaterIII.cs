using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class WaterIII : ISpecialAction
{
	public override string Name => "Water III";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37831u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.SourceId.GameObject()?.TargetObject, 8f, 3200f);
	}
}

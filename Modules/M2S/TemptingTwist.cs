using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M2S;

public class TemptingTwist : ISpecialAction
{
	public override string Name => "Tempting Twist";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39626u, 39697u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 7f, 30f);
	}
}

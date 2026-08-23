using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class Sabertail : ISpecialAction
{
	public override string Name => "Ground AoE";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38389u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineCircle(info, 6.5f, 700f, 6, ShowAll: true);
	}
}

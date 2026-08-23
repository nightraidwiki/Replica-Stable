using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BarbaricciaEx;

public class Megaboulder : ISpecialAction
{
	public override string Name => "Megaboulder";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30107u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 20f, 3000f, 0f, 30107u);
	}
}

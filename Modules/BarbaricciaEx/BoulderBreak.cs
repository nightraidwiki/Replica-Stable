using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BarbaricciaEx;

public class BoulderBreak : ISpecialAction
{
	public override string Name => "Boulder Break";

	public override HashSet<uint> ActionID => new HashSet<uint> { 29571u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 6f, 3000f, 0f, 29571u);
	}
}

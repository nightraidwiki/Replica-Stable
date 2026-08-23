using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.A4;

public class JudgmentRay : ISpecialAction
{
	public override string Name => "Judgment Ray";

	public override HashSet<uint> ActionID => new HashSet<uint> { 6884u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 5f, 3000f, 0f, 6884u);
	}
}

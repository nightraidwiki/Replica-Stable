using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class GreatDivide : ISpecialAction
{
	public override string Name => "Great Divide";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41944u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.RectangleToTarget(info, 60f, 3f);
	}
}

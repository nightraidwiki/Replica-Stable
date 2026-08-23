using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1NHaunted;

public class StardustImpact : ISpecialAction
{
	public override string Name => "Stardust Impact";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33077u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 15f, 3000f, 0f, 33077u);
	}
}

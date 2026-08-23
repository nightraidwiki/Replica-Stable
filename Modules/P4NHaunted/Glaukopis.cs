using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P4NHaunted;

public class Glaukopis : ISpecialAction
{
	public override string Name => "Glaukopis";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33493u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.RectangleToTarget(info, 60f, 2.5f);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4;

public class WickedJolt : ISpecialAction
{
	public override string Name => "Wicked Jolt";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37576u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.RectangleToTarget(info, 60f, 2.5f);
	}
}

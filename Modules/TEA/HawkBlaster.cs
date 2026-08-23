using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class HawkBlaster : ISpecialAction
{
	public override string Name => "Hawk Blaster";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18481u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(new Vector3(info.Pos.X, 0f, info.Pos.Z), 10f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 18481u }
		});
	}
}

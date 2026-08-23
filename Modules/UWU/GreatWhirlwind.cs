using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class GreatWhirlwind : ISpecialAction
{
	public override string Name => "Great Whirlwind";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11073u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 8f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 11073u }
		});
	}
}

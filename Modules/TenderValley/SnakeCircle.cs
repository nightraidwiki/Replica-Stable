using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TenderValley;

public class SnakeCircle : ISpecialAction
{
	public override string Name => "Snake (circle)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 36749u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 9f);
	}
}

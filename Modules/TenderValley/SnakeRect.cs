using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TenderValley;

public class SnakeRect : ISpecialAction
{
	public override string Name => "Snake (rect)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 36750u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 25.5f, 2.5f, 25.5f);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Everkeep;

public class HalfFull : ISpecialAction
{
	public override string Name => "Half Full";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37736u, 37737u };

	public override void OnActionCast(ActorCastInfo info)
	{
		Angle angle = info.Facing + ((info.ActionId == 37737) ? 90 : (-90)).Degrees();
		uint sourceId = info.SourceId;
		Angle rotation = angle;
		uint actionId = info.ActionId;
		SimpleElement.Rectangle(sourceId, 60f, 60f, 0f, null, rotation, 3000f, 0f, actionId);
	}
}

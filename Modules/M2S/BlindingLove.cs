using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M2S;

public class BlindingLove : ISpecialAction
{
	public override string Name => "Blinding Love";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39627u, 39628u, 39629u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 39627:
		case 39628:
		{
			uint sourceId2 = info.SourceId;
			Angle facing2 = info.Facing;
			SimpleElement.Rectangle(sourceId2, 60f, 5f, 0f, null, facing2, 4500f, 1500f);
			break;
		}
		case 39629:
		{
			uint sourceId = info.SourceId;
			Angle facing = info.Facing;
			SimpleElement.Rectangle(sourceId, 100f, 4f, 0f, null, facing, 4000f, 3000f);
			break;
		}
		}
	}
}

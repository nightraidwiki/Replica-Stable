using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.HoneyBLovely;

public class BlindingLove : ISpecialAction
{
	public override string Name => "Blinding Love";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39525u, 39526u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 39525)
		{
			uint sourceId = info.SourceId;
			Angle facing = info.Facing;
			SimpleElement.Rectangle(sourceId, 50f, 4f, 0f, null, facing, 4000f, 3000f);
		}
		if (info.ActionId == 39526)
		{
			SimpleElement.Rectangle(info, 50f, 4f);
		}
	}
}

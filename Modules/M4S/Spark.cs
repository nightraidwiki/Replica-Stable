using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class Spark : ISpecialAction
{
	public override string Name => "Spark";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38345u, 38346u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 38345:
		{
			uint sourceId2 = info.SourceId;
			Angle rotation2 = 0.Degrees();
			SimpleElement.Rectangle(sourceId2, 5f, 5f, 5f, null, rotation2, 3000f, 0f, 38345u);
			break;
		}
		case 38346:
		{
			uint sourceId = info.SourceId;
			Angle rotation = 0.Degrees();
			SimpleElement.Rectangle(sourceId, 15f, 15f, 15f, null, rotation, 3000f, 0f, 38346u);
			break;
		}
		}
	}
}

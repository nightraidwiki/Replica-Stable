using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DiamondWeapon;

public class ArmorDeployment : ISpecialAction
{
	public override string Name => "Armor Deployment";

	public override HashSet<uint> ActionID => new HashSet<uint> { 24474u, 24475u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 24474)
		{
			uint sourceId = info.SourceId;
			Angle rotation = info.Facing - 90.Degrees();
			SimpleElement.Rectangle(sourceId, 42f, 22f, 0f, null, rotation, 3000f, 0f, 24538u);
		}
		else
		{
			uint sourceId2 = info.SourceId;
			Angle rotation2 = info.Facing + 90.Degrees();
			SimpleElement.Rectangle(sourceId2, 42f, 22f, 0f, null, rotation2, 3000f, 0f, 24537u);
		}
	}
}

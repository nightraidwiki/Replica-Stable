using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ForkedTower;

public class Rotation : ISpecialAction
{
	public override string Name => "Rotation";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41731u, 41732u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 41731:
			SimpleElement.Fan(info.Pos, 37f, 90, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			break;
		case 41732:
			SimpleElement.Rectangle(info.Pos, 33f, 1.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			break;
		}
	}
}

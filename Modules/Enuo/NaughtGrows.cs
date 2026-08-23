using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Util;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Enuo;

public class NaughtGrows : ISpecialAction
{
	public override string Name => "Naught Grows";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		49977u, // NaughtGrowsCircle
		49978u, // NaughtGrowsDonut
		49979u, // NaughtGrowsBossCircle
		49980u, // NaughtGrowsBossDonut
		49985u, // PassageOfNaught
		49986u, // PassageOfNaught1
		49987u  // PassageOfNaught2
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 49977u)
		{
			SimpleElement.Circle(info, 40f);
		}
		else if (info.ActionId == 49978u)
		{
			SimpleElement.Donut(info, 40f, 60f);
		}
		else if (info.ActionId == 49979u)
		{
			SimpleElement.Circle(info, 12f);
		}
		else if (info.ActionId == 49980u)
		{
			SimpleElement.Donut(info, 6f, 40f);
		}
		else if (info.ActionId == 49985u || info.ActionId == 49986u || info.ActionId == 49987u)
		{
			SimpleElement.Rectangle(info, 80f, 8f);
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 701 || icon == 702)
		{
			var targetObj = TargetID.GameObject();
			if (Source != null && targetObj != null)
			{
				SimpleElement.RectangleToTarget(Source, targetObj, 80f, 3f, 7000f, new HitCounter
				{
					ActionID = new HashSet<uint> { 49983u, 49984u }
				});
			}
		}
	}
}

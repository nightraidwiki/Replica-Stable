using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class FairyPellyPhantomFlameDance : ISpecialAction
{
	public bool? isLeft;

	public override string Name => "Fairy Pelly Phantom Flame Dance";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 45438u, 45439u };

	private static HashSet<uint> carpetRushIds => new HashSet<uint> { 45432u, 45433u, 45442u, 45443u, 46573u, 46574u, 46950u, 46951u, 47020u, 47021u };

	public override void OnActionCast(ActorCastInfo info)
	{
		isLeft = info.ActionId == 45439;
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 355 && isLeft.HasValue)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = actorId.GameObject().Position,
				drawOnObject = false,
				targetPosition = targetId.GameObject().Position,
				radiusX = 40f,
				radiusZ = 40f,
				refOffsetZ = -40f,
				refOffsetRotation = (isLeft.Value ? 90.Degrees() : (-90.Degrees())),
				hitCounter = new HitCounter
				{
					ActionID = carpetRushIds
				}
			});
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (carpetRushIds.Contains(info.ActionId))
		{
			Reset();
		}
	}

	public override void Reset()
	{
		isLeft = null;
		base.Reset();
	}
}

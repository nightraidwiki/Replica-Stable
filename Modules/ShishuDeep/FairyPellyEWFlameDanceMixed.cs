using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuDeep;

public class FairyPellyEWFlameDanceMixed : ISpecialAction
{
	private bool isLeft;

	private Vector3? lastPos;

	public override string Name => "Fairy Pelly E/W Flame Dance (mixed)";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(47025u);
			hashSet.Add(47026u);
			foreach (uint carpetRushId in carpetRushIds)
			{
				hashSet.Add(carpetRushId);
			}
			return hashSet;
		}
	}

	private static HashSet<uint> carpetRushIds => new HashSet<uint> { 45432u, 45433u, 45442u, 45443u, 46573u, 46574u, 46950u, 46951u, 47020u, 47021u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 47025:
			isLeft = false;
			break;
		case 47026:
			isLeft = true;
			break;
		}
		base.NumCasts = 0;
		lastPos = info.Pos;
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 355 && lastPos.HasValue && actorId.GameObject().Position.AlmostEqual(lastPos.Value, 1f))
		{
			base.NumCasts++;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = actorId.GameObject().Position,
				drawOnObject = false,
				targetPosition = targetId.GameObject().Position,
				radiusX = 40f,
				radiusZ = 40f,
				refOffsetZ = -40f,
				refOffsetRotation = (isLeft ? 90.Degrees() : (-90.Degrees())),
				hitCounter = new HitCounter
				{
					ActionID = carpetRushIds,
					TargetHitCount = base.NumCasts
				}
			});
			lastPos = targetId.GameObject().Position;
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
		lastPos = null;
		base.Reset();
	}
}

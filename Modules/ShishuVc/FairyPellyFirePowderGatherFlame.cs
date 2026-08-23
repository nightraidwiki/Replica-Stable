using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.ShishuVc;

public class FairyPellyFirePowderGatherFlame : ISpecialAction
{
	public bool? isLeft;

	public bool? isShare;

	public override string Name => "Fairy Pelly Fire Powder / Gather Flame";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(45434u);
			hashSet.Add(45435u);
			hashSet.Add(45436u);
			hashSet.Add(45437u);
			foreach (uint carpetRushId in carpetRushIds)
			{
				hashSet.Add(carpetRushId);
			}
			return hashSet;
		}
	}

	private static HashSet<uint> carpetRushIds => new HashSet<uint> { 45432u, 45433u, 45442u, 45443u, 46573u, 46574u, 46950u, 46951u, 47020u, 47021u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 45437:
			isLeft = true;
			isShare = true;
			break;
		case 45436:
			isLeft = false;
			isShare = true;
			break;
		case 45435:
			isLeft = true;
			isShare = false;
			break;
		case 45434:
			isLeft = false;
			isShare = false;
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (!carpetRushIds.Contains(info.ActionId) || aoes.Count <= 0)
		{
			return;
		}
		aoes[0].Remove();
		aoes.RemoveAt(0);
		if (aoes.Count != 0)
		{
			return;
		}
		if (!isShare == true)
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn5m5s(allPlayer);
			}
		}
		else
		{
			SimpleLockon.Share5S(Svc.Objects.LocalPlayer);
		}
		Reset();
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 355 && isLeft.HasValue)
		{
			base.NumCasts++;
			DrawElement element = new DrawElement
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
					ActionID = carpetRushIds,
					TargetHitCount = base.NumCasts
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
	}

	public override void Reset()
	{
		isLeft = null;
		isShare = null;
		base.Reset();
	}
}

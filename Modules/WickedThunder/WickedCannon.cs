using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.WickedThunder;

public class WickedCannon : ISpecialAction
{
	private static readonly HashSet<uint> CastEnd = new HashSet<uint> { 20032u, 37550u, 37551u, 39852u, 39870u };

	private static int HitCount = 0;

	public override string Name => "Wicked Cannon";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(37549u);
			hashSet.Add(37552u);
			hashSet.Add(39759u);
			hashSet.Add(39765u);
			hashSet.Add(39766u);
			hashSet.Add(39767u);
			foreach (uint item in CastEnd)
			{
				hashSet.Add(item);
			}
			return hashSet;
		}
	}

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		base.NumCasts = 0;
		int hitCount;
		switch (info.ActionId)
		{
		case 37549:
		case 37552:
			hitCount = 3;
			break;
		case 39759:
		case 39765:
			hitCount = 4;
			break;
		case 39766:
		case 39767:
			hitCount = 5;
			break;
		default:
			hitCount = 0;
			break;
		}
		HitCount = hitCount;
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2970 && HitCount > base.NumCasts)
		{
			base.NumCasts++;
			Angle refRotation = info.Stack switch
			{
				723u => 180.Degrees(), 
				724u => 0.Degrees(), 
				_ => 0.Degrees(), 
			};
			DrawElement element = new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 5f,
				radiusZ = 40f,
				drawOnObject = true,
				refRotation = refRotation,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = CastEnd,
					TargetHitCount = base.NumCasts
				}
			};
			aoes.Add(DrawManager.Draw(element, info.TargetID.GameObject()));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (CastEnd.Contains(info.ActionId) && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

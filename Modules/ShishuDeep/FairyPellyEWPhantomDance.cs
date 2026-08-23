using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.ShishuDeep;

public class FairyPellyEWPhantomDance : ISpecialAction
{
	private readonly List<IGameObject> left = new List<IGameObject>();

	private readonly List<IGameObject> right = new List<IGameObject>();

	public override string Name => "Fairy Pelly E/W Phantom Dance";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(45428u);
			hashSet.Add(45429u);
			hashSet.Add(46946u);
			hashSet.Add(46947u);
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
		case 45428:
		case 46946:
			right.Add(info.SourceId.GameObject());
			break;
		case 45429:
		case 46947:
			left.Add(info.SourceId.GameObject());
			break;
		}
		base.NumCasts = 0;
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 355 && (left.Count != 0 || right.Count != 0))
		{
			base.NumCasts++;
			bool flag = left.Any((IGameObject o) => o.Position.AlmostEqual(actorId.GameObject().Position, 1f));
			DrawElement element = new DrawElement
			{
				drawAvfx = "general02xf",
				Position = actorId.GameObject().Position,
				drawOnObject = false,
				targetPosition = targetId.GameObject().Position,
				radiusX = 40f,
				radiusZ = 40f,
				refOffsetZ = -40f,
				refOffsetRotation = (flag ? 90.Degrees() : (-90.Degrees())),
				hitCounter = new HitCounter
				{
					ActionID = carpetRushIds,
					TargetHitCount = base.NumCasts
				}
			};
			aoes.Add(DrawManager.Draw(element));
			if (flag)
			{
				left.Add(targetId.GameObject());
			}
			else
			{
				right.Add(targetId.GameObject());
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (carpetRushIds.Contains(info.ActionId))
		{
			if (aoes.Count > 0)
			{
				aoes[0].Remove();
				aoes.RemoveAt(0);
			}
			left.Clear();
			right.Clear();
			base.NumCasts = 0;
		}
	}

	public override void Reset()
	{
		left.Clear();
		right.Clear();
		base.Reset();
	}
}

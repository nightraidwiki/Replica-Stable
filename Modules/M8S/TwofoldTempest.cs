using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class TwofoldTempest : ISpecialAction
{
	public override string Name => "Twofold Tempest";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42097u, 42099u };

	public override void Update()
	{
		List<IGameObject> list = PlayerHelper.AllPlayers.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			IGameObject gameObject = list[i];
			for (int j = 0; j < 5; j++)
			{
				WPos origin = new WPos(HowlingBlade.EndArenaPlatforms[j]);
				if (new WPos(gameObject.Position).InCircle(origin, 8f) && aoes.Count > i)
				{
					aoes[i].TargetPosition = origin.ToVec3(-150f);
					break;
				}
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 42097)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				radiusX = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 84 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42098u },
					TargetHitCount = 4
				}
			};
			DrawManager.Draw(element, allPlayer);
			element = new DrawElement
			{
				drawAvfx = "general02pxf",
				Position = new Vector3(100f, -150f, 100f),
				drawOnObject = false,
				targetPosition = Vector3.Zero,
				radiusX = 8f,
				radiusZ = 40f,
				distanceCheck = new DistanceCheck
				{
					CheckObject = allPlayer,
					Position = new Vector3(100f, -150f, 100f),
					CheckType = 8
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42099u },
					TargetHitCount = 4
				}
			};
			aoes.Add(DrawManager.Draw(element, allPlayer));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 42099 && ++base.NumCasts == 4)
		{
			aoes.Clear();
		}
	}
}

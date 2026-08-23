using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.CloudOfDarkness;

public class P2PullSteelDonut : ISpecialAction
{
	public override string Name => "Pull center steel donut";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40515u, 40517u, 40518u };

	public override uint Phase => 2u;

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (ModuleUtil.GetSpecialAction<Razing_volleyParticleBeam>().aoes.Count > 0)
			{
				return Array.Empty<StaticVfx>();
			}
			return aoes.Take(1);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 40515)
		{
			DrawElement element = new DrawElement
			{
				Enable = false,
				drawAvfx = "general_1bxf",
				Position = new Vector3(100f, 0f, 76.28425f),
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40517u }
				}
			};
			aoes.Add(DrawManager.Draw(element));
			DrawElement element2 = new DrawElement
			{
				Enable = false,
				drawAvfx = "customDonut",
				Position = new Vector3(100f, 0f, 76.28425f),
				drawOnObject = false,
				refRadian = 0.15f,
				radiusX = 40f,
				radiusZ = 40f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40518u }
				},
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor
			};
			aoes.Add(DrawManager.Draw(element2));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 40517 <= 1 && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

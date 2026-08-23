using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M4S;

public class TailThrustKnockback : ISpecialAction
{
	public override string Name => "Tail Thrust (knockback)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38407u, 38409u, 38415u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 38407 || actionId == 38409)
		{
			DrawElement element = new DrawElement
			{
				Enable = false,
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 25f,
				drawOnObject = true,
				KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3((info.ActionId == 38407) ? 90 : 110, 0f, 165f),
					Antiable = false
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u }
				}
			};
			aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
			DrawElement element2 = new DrawElement
			{
				Enable = false,
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 25f,
				drawOnObject = true,
				KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3((info.ActionId == 38407) ? 110 : 90, 0f, 165f),
					Antiable = false
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u },
					TargetHitCount = 2
				}
			};
			aoes.Add(DrawManager.Draw(element2, Svc.Objects.LocalPlayer));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 38415 && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

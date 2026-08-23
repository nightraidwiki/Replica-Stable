using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.CloudOfDarkness;

public class P2KnockbackDeathCircle : ISpecialAction
{
	public override string Name => "Knockback center death circle";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40524u };

	public override uint Phase => 2u;

	public override void Update()
	{
		int count = ModuleUtil.GetSpecialAction<Razing_volleyParticleBeam>().aoes.Count;
		if (aoes.Count <= 0)
		{
			return;
		}
		foreach (StaticVfx item in aoes.ToList())
		{
			if (item != null)
			{
				item.Enable = count <= 0;
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			Enable = false,
			drawAvfx = "general_1bxf",
			Position = new Vector3(100f, 0f, 76.28425f),
			drawOnObject = false,
			radiusX = 8f,
			radiusZ = 8f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40523u }
			}
		};
		aoes.Add(DrawManager.Draw(element));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

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

public class BladeofDarkness : ISpecialAction
{
	public override string Name => "Blade of Darkness";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40444u, 40446u, 40448u };

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		int count = ModuleUtil.GetSpecialAction<Razing_volleyParticleBeam>().aoes.Count;
		foreach (StaticVfx item in aoes.ToList())
		{
			if (item != null)
			{
				item.Color = new Vector4(1f, 1f, 1f, (count > 0) ? 0.4f : Plugin.Config.CustomAlpha);
				item.TargetColor = new Vector4(1f, 1f, 1f, (count > 0) ? 0.4f : Plugin.Config.CustomAlpha);
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40444:
		case 40446:
		{
			DrawElement element2 = new DrawElement
			{
				drawAvfx = "gl_fan150_6012x",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 60f,
				radiusZ = 60f,
				refRotation = info.Facing,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			};
			aoes.Add(DrawManager.Draw(element2));
			break;
		}
		case 40448:
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "gl_fan180_1bf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 30f,
				radiusZ = 30f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			};
			aoes.Add(DrawManager.Draw(element));
			break;
		}
		case 40445:
		case 40447:
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

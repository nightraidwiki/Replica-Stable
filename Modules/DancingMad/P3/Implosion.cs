using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingMad.P3;

public class Implosion : ISpecialAction
{
	public override string Name => "Implosion";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47869u, 47870u, 47871u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 47869 || info.ActionId == 47870)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 40f,
				radiusZ = 40f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47871u },
					TargetHitCount = 4
				}
			};
			if (info.ActionId == 47869)
			{
				drawElement.refRotation = info.Facing;
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing + 180.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing + 90.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing - 90.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
			}
			else
			{
				drawElement.refRotation = info.Facing + 90.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing - 90.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing;
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation = info.Facing + 180.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 47871 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

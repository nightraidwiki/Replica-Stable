using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.GolbezEx;

public class HeadlightThunderousBreath : ISpecialAction
{
	public override string Name => "Headlight / Thunderous Breath";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45687u, 45689u, 45690u, 45692u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "mdl_general03_o0e1",
			drawOnObject = true,
			radiusX = 35f,
			radiusZ = 70f,
			refOffsetY = ((info.ActionId != 45687) ? (-5) : 0),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 45689u, 45692u },
				TargetHitCount = 2
			}
		};
		aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
		drawElement.refOffsetY = ((info.ActionId == 45687) ? (-5) : 0);
		aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		if ((actionId == 45689 || actionId == 45692) && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

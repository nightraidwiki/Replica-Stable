using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingMad.P5;

public class Flood : ISpecialAction
{
	public override string Name => "Flood";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 49539u, 49769u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(4);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 49539)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "general02xf",
				Position = info.Pos,
				drawOnObject = false,
				refRotation = info.Facing,
				radiusX = 5f,
				radiusZ = 40f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 49769u },
					TargetHitCount = 8
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 49769 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

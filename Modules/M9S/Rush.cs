using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M9S;

public class Rush : ISpecialAction
{
	public override string Name => "Rush";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45928u, 45929u, 45930u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "general02xf",
			Position = info.Pos,
			drawOnObject = false,
			radiusX = 2.5f,
			radiusZ = 32f,
			refRotation = info.Facing,
			destroyTime = info.CastTime * 1000f
		};
		aoes.Add(DrawManager.Draw(element));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

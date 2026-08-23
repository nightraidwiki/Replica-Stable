using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.SugarRiot;

public class SprayPain : ISpecialAction
{
	public override string Name => "Spray Pain";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42603u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "m0347_sircle_01m1",
			radiusX = 10f,
			radiusZ = 10f,
			delayDrawTime = 7000f
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

using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M6S;

public class SprayPain : ISpecialAction
{
	public override string Name => "Spray Pain";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42657u, 39468u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 42657)
		{
			aoes.Add(SimpleElement.Circle(info.SourceId, 10f, 7000f));
			return;
		}
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "m0347_sircle_01m1",
			radiusX = 10f,
			radiusZ = 10f,
			drawOnObject = true,
			destroyTime = 8500f
		}, info.SourceId.GameObject());
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 42657 && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

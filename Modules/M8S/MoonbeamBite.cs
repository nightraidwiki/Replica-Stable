using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M8S;

public class MoonbeamBite : ISpecialAction
{
	public override string Name => "Moonbeam Bite";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41923u, 41922u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info.Pos, 40f, 10f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID,
			TargetHitCount = 4
		}));
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

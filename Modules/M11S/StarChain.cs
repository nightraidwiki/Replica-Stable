using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M11S;

public class StarChain : ISpecialAction
{
	public override string Name => "Star Chain";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46131u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info.Pos, 60f, 5f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID,
			TargetHitCount = 8
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

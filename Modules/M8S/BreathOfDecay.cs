using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M8S;

public class BreathOfDecay : ISpecialAction
{
	public override string Name => "Breath Of Decay";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41908u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info.Pos, 40f, 4f, 0f, info.Facing, 8000f));
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

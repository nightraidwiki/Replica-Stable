using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M7S;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion Explosion";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42358u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Circle(info.Pos, 25f, 9000f));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

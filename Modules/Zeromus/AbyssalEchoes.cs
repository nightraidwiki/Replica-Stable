using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.Zeromus;

public class AbyssalEchoes : ISpecialAction
{
	public override string Name => "Abyssal Echoes";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35650u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Circle(info.TargetId, 12f, 16000f));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

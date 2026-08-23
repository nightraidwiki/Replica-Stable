using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.SpheneDarkEx;

public class AzureRing : ISpecialAction
{
	public override string Name => "Azure Ring";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44600u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Donut(info.Pos, 3f, 50f, 7000f));
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

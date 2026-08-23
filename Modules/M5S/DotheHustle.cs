using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M5S;

public class DotheHustle : ISpecialAction
{
	public override string Name => "Do the Hustle";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42869u, 42870u, 42789u, 42788u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((uint)(info.ActionId - 42869) <= 1u)
		{
			aoes.Add(SimpleElement.Fan(info.SourceId, 50f, 180, info.Facing, info.CastTime * 1000f));
		}
		else
		{
			SimpleElement.Fan(info, 180);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 42869 <= 1 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

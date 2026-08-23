using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class TendrilsOfTerror : ISpecialAction
{
	public override string Name => "Tendrils of Terror";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42352u, 42394u, 42397u };

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.AddRange(SimpleElement.Cross(info));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

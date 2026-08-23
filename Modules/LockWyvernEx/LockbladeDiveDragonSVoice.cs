using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class LockbladeDiveDragonSVoice : ISpecialAction
{
	public override string Name => "Lockblade Dive (Dragon's Voice)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43903u, 45101u };

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

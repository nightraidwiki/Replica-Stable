using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class LockbladeStrike : ISpecialAction
{
	public override string Name => "Lockblade Strike";

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		43889u, 43890u, 45077u, 45078u, 45086u, 45087u, 45083u, 45084u, 45079u, 45080u,
		43895u, 43896u, 45091u, 45092u, 45094u, 45095u
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

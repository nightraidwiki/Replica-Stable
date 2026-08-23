using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class TailThrust : ISpecialAction
{
	public override string Name => "Tail Thrust";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44805u, 45109u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

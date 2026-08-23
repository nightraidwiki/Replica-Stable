using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class LockbladeDiveResonance : ISpecialAction
{
	public override string Name => "Lockblade Dive (Resonance)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43900u, 45099u, 43901u, 45100u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

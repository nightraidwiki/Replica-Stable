using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class SpinningSlashDragonSVoice : ISpecialAction
{
	public override string Name => "Spinning Slash (Dragon's Voice)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43916u, 43918u, 45107u, 45108u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

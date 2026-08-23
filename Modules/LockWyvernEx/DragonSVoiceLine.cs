using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class DragonSVoiceLine : ISpecialAction
{
	public override string Name => "Dragon's Voice (line)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43940u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineRect(info, 8f, 2500f, 5);
	}
}

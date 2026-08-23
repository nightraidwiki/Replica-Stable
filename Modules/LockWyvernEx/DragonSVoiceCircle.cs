using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class DragonSVoiceCircle : ISpecialAction
{
	public override string Name => "Dragon's Voice (circle)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43926u, 43952u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineCircle(info, 8f, 1100f, (info.ActionId == 43926) ? 2 : 5);
	}
}

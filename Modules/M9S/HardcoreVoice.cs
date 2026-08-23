using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M9S;

public class HardcoreVoice : ISpecialAction
{
	public override string Name => "Hardcore Voice";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45951u, 45952u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId.GameObject(), (info.ActionId == 45951) ? 6 : 15, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

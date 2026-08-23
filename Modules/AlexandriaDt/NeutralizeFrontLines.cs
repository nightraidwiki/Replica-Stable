using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class NeutralizeFrontLines : ISpecialAction
{
	public override string Name => "Neutralize Front Lines";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42738u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.Pos, 30f, 180, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

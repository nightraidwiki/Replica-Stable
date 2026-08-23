using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class Electray : ISpecialAction
{
	public override string Name => "Lightning Ray";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43130u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info.Pos, 40f, 4.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

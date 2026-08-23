using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class AlmightyRacket : ISpecialAction
{
	public override string Name => "Almighty Racket";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42546u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info.Pos, 30f, 15f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.AlexandriaDt;

public class AerialAmbush : ISpecialAction
{
	public override string Name => "Aerial Ambush";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42543u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info.Pos, 30f, 7.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

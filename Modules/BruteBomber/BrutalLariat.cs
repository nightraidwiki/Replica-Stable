using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BruteBomber;

public class BrutalLariat : ISpecialAction
{
	public override string Name => "Brutal Lariat";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39638u, 39639u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 50f, 17f, 50f, (info.ActionId == 39638) ? new Vector2(12f, 0f) : new Vector2(-12f, 0f));
	}
}

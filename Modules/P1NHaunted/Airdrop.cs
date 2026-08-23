using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1NHaunted;

public class Airdrop : ISpecialAction
{
	public override string Name => "Airdrop";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33094u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 14f, 3000f, 0f, 33094u);
	}
}

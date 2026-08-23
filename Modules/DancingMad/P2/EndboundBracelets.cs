using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P2;

public class EndboundBracelets : ISpecialAction
{
	public override string Name => "Endbound Bracelets";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 49740u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 5f, 3000f, 0f, 49740u);
	}
}

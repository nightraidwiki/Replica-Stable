using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ForkedTower;

public class FrigidDive : ISpecialAction
{
	public override string Name => "Frigid Dive";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37819u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

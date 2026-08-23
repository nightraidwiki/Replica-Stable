using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class P2CrimsonCyclone : ISpecialAction
{
	public override string Name => "Ifrit Crimson Cyclone";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11103u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 49f, 9f, 5f);
	}
}

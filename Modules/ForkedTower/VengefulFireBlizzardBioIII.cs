using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ForkedTower;

public class VengefulFireBlizzardBioIII : ISpecialAction
{
	public override string Name => "Vengeful Fire/Blizzard/Bio III";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42429u, 42430u, 42431u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

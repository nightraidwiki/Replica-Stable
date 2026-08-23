using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DiamondWeapon;

public class AetherCannon : ISpecialAction
{
	public override string Name => "Aether Cannon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 24533u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

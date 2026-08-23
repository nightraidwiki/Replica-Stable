using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UltimaWeapon;

public class SiegeCannon : ISpecialAction
{
	public override string Name => "Siege Cannon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 29020u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

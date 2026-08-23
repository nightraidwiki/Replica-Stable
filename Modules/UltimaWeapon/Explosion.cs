using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UltimaWeapon;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion";

	public override HashSet<uint> ActionID => new HashSet<uint> { 29021u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 16f);
	}
}

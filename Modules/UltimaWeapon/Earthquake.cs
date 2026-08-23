using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UltimaWeapon;

public class Earthquake : ISpecialAction
{
	public override string Name => "Earthquake";

	public override HashSet<uint> ActionID => new HashSet<uint> { 29000u, 28981u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

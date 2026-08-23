using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UltimaWeapon;

public class EyeOfTheTyphoon : ISpecialAction
{
	public override string Name => "Eye of the Typhoon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 28980u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 12.5f, 25f);
	}
}

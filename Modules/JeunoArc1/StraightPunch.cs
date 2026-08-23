using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.JeunoArc1;

public class StraightPunch : ISpecialAction
{
	public override string Name => "Straight Punch";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40939u, 40940u, 40941u, 40942u, 40943u, 40944u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40939:
			aoes.Add(SimpleElement.Circle(info, 9f));
			break;
		case 40940:
			aoes.Add(SimpleElement.Circle(info, 18f));
			break;
		case 40941:
			aoes.Add(SimpleElement.Circle(info, 27f));
			break;
		case 40942:
			aoes.Add(SimpleElement.Donut(info, 9f, 60f));
			break;
		case 40943:
			aoes.Add(SimpleElement.Donut(info, 18f, 60f));
			break;
		case 40944:
			aoes.Add(SimpleElement.Donut(info, 27f, 60f));
			break;
		}
		aoes.SortBy((StaticVfx x) => x.DrawTime);
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

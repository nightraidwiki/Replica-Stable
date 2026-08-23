using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M7S;

public class BrutishSwing : ISpecialAction
{
	public override string Name => "Brutish Swing";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42337u, 42403u, 42386u, 42338u, 42387u, 42405u };

	public override void Update()
	{
		if (aoes.Count != 0)
		{
			SporeCloud? specialAction = ModuleUtil.GetSpecialAction<SporeCloud>();
			TendrilsOfTerror specialAction2 = ModuleUtil.GetSpecialAction<TendrilsOfTerror>();
			bool flag = specialAction.aoes.Count > 0 || specialAction2.aoes.Count > 0;
			aoes[0].Color = new Vector4(1f, 1f, 1f, flag ? 0.3f : Plugin.Config.CustomAlpha);
			aoes[0].TargetColor = new Vector4(1f, 1f, 1f, flag ? 0.3f : Plugin.Config.CustomAlpha);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 42337:
			SimpleElement.Circle(info);
			break;
		case 42386:
		case 42403:
			SimpleElement.Fan(info);
			break;
		case 42338:
		case 42387:
		case 42405:
			aoes.Add(SimpleElement.Donut(info));
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

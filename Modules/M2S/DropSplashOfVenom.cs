using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M2S;

public class DropSplashOfVenom : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Spread,
		Stack
	}

	private Mechanic nextMechanic;

	public override string Name => "Drop of Venom / Splash of Venom";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37252u, 37253u, 39688u, 39689u, 39625u, 39626u, 39696u, 39697u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37252:
		case 39688:
			nextMechanic = Mechanic.Spread;
			SimpleElement.ShowText("Spread soon");
			break;
		case 37253:
		case 39689:
			nextMechanic = Mechanic.Stack;
			SimpleElement.ShowText("2+2 stacks soon");
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 39625u:
		case 39626u:
		case 39696u:
		case 39697u:
			switch (nextMechanic)
			{
			case Mechanic.Stack:
				foreach (IGameObject item in PlayerHelper.Tank)
				{
					SimpleLockon.ShareLockon2_6m(item);
				}
				{
					foreach (IGameObject item2 in PlayerHelper.Healer)
					{
						SimpleLockon.ShareLockon2_6m(item2);
					}
					break;
				}
			case Mechanic.Spread:
			{
				foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
				{
					SimpleLockon.TarLockOn6m5s(allPlayer);
				}
				break;
			}
			}
			break;
		case 37257u:
		case 37258u:
		case 39694u:
		case 39695u:
			nextMechanic = Mechanic.None;
			break;
		}
	}

	public override void Reset()
	{
		nextMechanic = Mechanic.None;
	}
}

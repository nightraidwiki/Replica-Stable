using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class Quietus : ISpecialAction
{
	public override string Name => "Quietus";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40283u, 40284u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (Svc.Objects.LocalPlayer.GetRole() == CombatRole.Tank)
		{
			SimpleElement.ShowText("Bait execution");
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 8f,
				radiusZ = 8f,
				drawOnObject = true,
				delayDrawTime = 1000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40284u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = 3
				}
			}, allPlayer);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 40284)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 8f,
				radiusZ = 8f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40285u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.Source,
					CheckType = 2
				}
			}, allPlayer);
		}
	}
}

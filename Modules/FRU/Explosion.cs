using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class Explosion : ISpecialAction
{
	public override string Name => "Wings of Light/Dark (tether)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40313u, 40233u };

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 1 || Id == 2)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 3f,
				radiusZ = 3f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = actorId.GameObject(),
					CheckType = ((Id == 1) ? 2 : 3)
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40320u }
				}
			}, targetId.GameObject());
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (PlayerHelper.Tank.FirstOrDefault((IGameObject o) => o != WingsOfLightDarkTankCleave.MT) == Svc.Objects.LocalPlayer)
		{
			switch (info.ActionId)
			{
			case 40313:
				SimpleElement.ShowText("Light — bait far");
				break;
			case 40233:
				SimpleElement.ShowText("Dark — bait close");
				break;
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (WingsOfLightDarkTankCleave.MT == Svc.Objects.LocalPlayer)
		{
			switch (info.ActionId)
			{
			case 40313u:
				SimpleElement.ShowText("Dark — bait close");
				break;
			case 40233u:
				SimpleElement.ShowText("Light — bait far");
				break;
			}
		}
	}
}

using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class HandOfDeath : ISpecialAction
{
	public override string Name => "Hand of Death";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44551u, 44567u, 44578u, 44579u, 44584u };

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 18700 && (GameObject.Position.X != 100f || GameObject.Position.Z != 100f))
		{
			SimpleElement.Circle(GameObject.Position, 3f, 7800f);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 44578)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 3f,
				radiusZ = 24f,
				target = info.TargetId.GameObject(),
				destroyTime = 3000f
			}, info.SourceId.GameObject());
		}
		ushort actionId = info.ActionId;
		if (actionId == 44567 || actionId == 44579 || actionId == 44584)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 3f,
				radiusZ = 24f,
				refRotation = info.Facing,
				fixRotation = true,
				destroyTime = 3000f
			}, info.SourceId.GameObject());
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 44551)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 3f,
				radiusZ = 24f,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckType = 4,
					Position = info.Pos
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 44552u }
				}
			});
		}
	}
}

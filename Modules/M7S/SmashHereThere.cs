using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M7S;

public class SmashHereThere : ISpecialAction
{
	public override string Name => "Smash Here / There";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42335u, 42336u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 6f,
				radiusZ = 6f,
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = ((info.ActionId == 42335) ? 2 : 3)
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42341u, 42342u }
				}
			}, allPlayer);
		}
	}
}

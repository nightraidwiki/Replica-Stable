using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class SuperJump : ISpecialAction
{
	public override string Name => "Super Jump (bait)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18505u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 10f,
				radiusZ = 10f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = 3
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18506u }
				}
			}, allPlayer);
		}
	}
}

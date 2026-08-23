using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M10S;

public class DeepseaImpact : ISpecialAction
{
	public override string Name => "Deepsea Impact";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46519u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckType = 3,
					Count = 1,
					CheckObject = info.SourceId.GameObject()
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 44486u }
				}
			}, allPlayer);
		}
	}
}

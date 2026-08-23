using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Body;

public class ZenithStrike : ISpecialAction
{
	public override string Name => "Zenith Strike";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46301u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "share2_6m",
				radiusX = 6f,
				radiusY = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46302u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckType = 2,
					Count = 1,
					CheckObject = info.SourceId.GameObject()
				}
			}, allPlayer);
		}
	}
}

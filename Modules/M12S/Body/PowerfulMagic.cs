using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Body;

public class PowerfulMagic : ISpecialAction
{
	public override string Name => "Powerful Magic";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46303u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 5f,
				radiusY = 6f,
				radiusZ = 5f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46304u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckType = 2,
					Count = 2,
					CheckObject = info.SourceId.GameObject()
				}
			}, allPlayer);
		}
	}
}

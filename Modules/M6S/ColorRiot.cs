using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class ColorRiot : ISpecialAction
{
	public override string Name => "Color Riot";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42641u, 42642u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "customCircle",
				radiusX = 4f,
				radiusZ = 4f,
				drawOnObject = true,
				refColor = ((info.ActionId == 42641) ? GroundOmen.Blue : GroundOmen.Red),
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42643u, 42644u }
				}
			};
			DrawManager.Draw(obj, allPlayer);
			obj.refColor = ((info.ActionId == 42641) ? GroundOmen.Red : GroundOmen.Blue);
			obj.distanceCheck = new DistanceCheck
			{
				CheckObject = info.SourceId.GameObject(),
				CheckType = 3
			};
			DrawManager.Draw(obj, allPlayer);
		}
	}
}

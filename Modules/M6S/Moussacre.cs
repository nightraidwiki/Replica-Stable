using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class Moussacre : ISpecialAction
{
	public override string Name => "Moussacre";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42682u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = 0,
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42683u }
				}
			}, info.SourceId.GameObject());
		}
	}
}

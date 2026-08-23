using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M11S;

public class QuadSpinFire : ISpecialAction
{
	public override string Name => "Quad Spin Fire";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46170u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				target = allPlayer,
				radiusZ = 60f,
				radiusX = 3f,
				distanceCheck = new DistanceCheck
				{
					CheckType = 0,
					Count = 4,
					CheckObject = info.SourceId.GameObject()
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46171u }
				}
			}, info.SourceId.GameObject());
		}
	}
}

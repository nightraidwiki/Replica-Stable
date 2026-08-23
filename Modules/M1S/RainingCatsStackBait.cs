using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class RainingCatsStackBait : ISpecialAction
{
	public override string Name => "Raining Cats (stack bait)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39611u, 39612u, 39613u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		if (gameObject == null)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 4f,
				radiusZ = 4f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38047u },
					TargetHitCount = 2
				}
			};
			DrawElement element2 = new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 4f,
				radiusZ = 4f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 3
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38047u },
					TargetHitCount = 2
				}
			};
			DrawManager.Draw(element, allPlayer);
			DrawManager.Draw(element2, allPlayer);
		}
	}
}

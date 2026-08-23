using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class WitchHunt : ISpecialAction
{
	public override string Name => "Witch Hunt (bait)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38366u };

	public override void OnActionCast(ActorCastInfo info)
	{
		bool flag = StatusHelper.GetParam(info.SourceId, 2970u, out var param) && param == 758;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				drawOnObject = true,
				radiusX = 6f,
				radiusZ = 6f,
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = (flag ? 2 : 3),
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38375u }
				}
			}, allPlayer);
		}
		if (!Svc.Objects.LocalPlayer.HasStatus(587u))
		{
			SimpleElement.ShowText((flag ? "Move in" : "Move out") + "(bait)");
		}
	}
}

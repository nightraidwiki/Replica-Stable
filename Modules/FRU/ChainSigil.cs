using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class ChainSigil : ISpecialAction
{
	public override string Name => "Chain (sigil)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 4166)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				drawOnObject = true,
				radiusX = 10f,
				radiusZ = 10f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40169u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.TargetID.GameObject(),
					CheckType = 2
				},
				delayDrawTime = 10000f
			}, allPlayer);
		}
		SimpleElement.Circle(info.TargetID.GameObject(), 10f, 3000f, 10000f, new HitCounter
		{
			ActionID = new HashSet<uint> { 40169u }
		});
	}
}

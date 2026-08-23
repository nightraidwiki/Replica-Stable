using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P2;

public class DestructionWing : ISpecialAction
{
	public override string Name => "Destruction Wing";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47821u, 47822u, 50311u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 47821 || info.ActionId == 47822)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02wf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 20f,
				radiusZ = 80f,
				refRotation = info.Facing,
				hitCounter = new HitCounter
				{
					ActionID = ActionID
				}
			});
		}
		if (info.ActionId != 50311)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "tank_lockon_5m_5s_noc",
				refColor = GroundOmen.Red,
				refTargetColor = GroundOmen.Red,
				radiusX = 7f,
				radiusZ = 7f,
				distanceCheck = new DistanceCheck
				{
					CheckType = 2,
					CheckObject = info.SourceId.GameObject()
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47823u }
				}
			};
			DrawManager.Draw(obj, allPlayer);
			obj.distanceCheck = new DistanceCheck
			{
				CheckType = 3,
				CheckObject = info.SourceId.GameObject()
			};
			DrawManager.Draw(obj, allPlayer);
		}
	}
}

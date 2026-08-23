using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class WitchHuntBait : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Near,
		Far
	}

	private Mechanic curMechanic;

	public override string Name => "Witch Hunt (bait)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38368u, 38369u, 19729u, 19730u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((uint)(info.ActionId - 38368) <= 1u)
		{
			base.NumCasts = 0;
		}
		base.NumCasts++;
		if (base.NumCasts == 1)
		{
			curMechanic = ((StatusHelper.GetParam(info.SourceId, 2970u, out var param) && param == 758) ? Mechanic.Near : Mechanic.Far);
		}
		else
		{
			curMechanic = ((curMechanic != Mechanic.Near) ? Mechanic.Near : Mechanic.Far);
		}
		if (base.NumCasts > 4)
		{
			return;
		}
		new TimeHelper(1500L, delegate
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					distanceCheck = new DistanceCheck
					{
						CheckObject = info.SourceId.GameObject(),
						CheckType = ((curMechanic == Mechanic.Near) ? 2 : 3),
						Count = 2
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 38372u }
					}
				}, allPlayer);
			}
		});
	}
}

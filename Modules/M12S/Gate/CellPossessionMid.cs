using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Gate;

public class CellPossessionMid : ISpecialAction
{
	public Dictionary<int, IGameObject> AlphaGroup = new Dictionary<int, IGameObject>();

	public Dictionary<int, IGameObject> BetaGroup = new Dictionary<int, IGameObject>();

	public int SnakeCounter;

	public bool DrawEnabled;

	public override string Name => "Cell Possession (Mid)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 48830u, 46268u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 48830)
		{
			AlphaGroup.Clear();
			BetaGroup.Clear();
			SnakeCounter = 0;
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		switch (info.StatusID)
		{
		case 3004u:
			if (info.TargetID.GameObject().HasStatus(4752u))
			{
				AlphaGroup.Add(1, info.TargetID.GameObject());
			}
			if (info.TargetID.GameObject().HasStatus(4754u))
			{
				BetaGroup.Add(1, info.TargetID.GameObject());
			}
			break;
		case 3005u:
			if (info.TargetID.GameObject().HasStatus(4752u))
			{
				AlphaGroup.Add(2, info.TargetID.GameObject());
			}
			if (info.TargetID.GameObject().HasStatus(4754u))
			{
				BetaGroup.Add(2, info.TargetID.GameObject());
			}
			break;
		case 3006u:
			if (info.TargetID.GameObject().HasStatus(4752u))
			{
				AlphaGroup.Add(3, info.TargetID.GameObject());
			}
			if (info.TargetID.GameObject().HasStatus(4754u))
			{
				BetaGroup.Add(3, info.TargetID.GameObject());
			}
			break;
		case 3007u:
			if (info.TargetID.GameObject().HasStatus(4752u))
			{
				AlphaGroup.Add(4, info.TargetID.GameObject());
			}
			if (info.TargetID.GameObject().HasStatus(4754u))
			{
				BetaGroup.Add(4, info.TargetID.GameObject());
			}
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 46268)
		{
			SnakeCounter++;
			_ = SnakeCounter;
		}
	}
}

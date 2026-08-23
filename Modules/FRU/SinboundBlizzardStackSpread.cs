using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class SinboundBlizzardStackSpread : ISpecialAction
{
	public override string Name => "Sinbound Blizzard (stack / spread)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40144u, 40148u, 40329u, 40330u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40144:
		case 40329:
		{
			foreach (IGameObject dP in PlayerHelper.DPS)
			{
				SimpleLockon.ShareLockon2(dP, (info.ActionId == 40144) ? 4200 : 4700);
			}
			break;
		}
		case 40148:
		case 40330:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn6m5s(allPlayer, (info.ActionId == 40148) ? 4200 : 4700);
			}
			break;
		}
		}
	}
}

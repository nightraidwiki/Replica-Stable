using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuVc;

public class FairyPellyPairSpreadFireBlossom : ISpecialAction
{
	public override string Name => "Fairy Pelly Pair/Spread Fire Blossom";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 45536u, 45538u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 45536:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn5m5s(allPlayer, 3000f);
			}
			break;
		}
		case 45538:
		{
			foreach (IGameObject dP in PlayerHelper.DPS)
			{
				SimpleLockon.ShareLockon2(dP, 3000f);
			}
			break;
		}
		}
	}
}

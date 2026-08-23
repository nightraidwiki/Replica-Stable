using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class PowerfulBanishStackSpread : ISpecialAction
{
	public override string Name => "Powerful Banish (stack / spread)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40220u, 40221u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40220:
		{
			foreach (IGameObject item in PlayerHelper.Tank.Union(PlayerHelper.Healer))
			{
				SimpleLockon.ShareLockon2(item);
			}
			break;
		}
		case 40221:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn5m5s(allPlayer);
			}
			break;
		}
		}
	}
}

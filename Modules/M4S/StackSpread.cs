using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class StackSpread : ISpecialAction
{
	public override string Name => "Stack / Spread";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38380u, 38381u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (StatusHelper.GetParam(info.SourceId, 2970u, out var param) && param == 752)
		{
			bool flag = false;
			foreach (IBattleChara allPlayer in PlayerHelper.AllPlayers)
			{
				if (allPlayer.StatusList.Any((IStatus x) => x.StatusId == 3999))
				{
					SimpleLockon.ShareLockon2_6m(allPlayer, (info.CastTime - 5f) * 1000f);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			{
				foreach (IGameObject item in PlayerHelper.Tank.Union(PlayerHelper.Healer))
				{
					SimpleLockon.ShareLockon2_6m(item, (info.CastTime - 5f) * 1000f);
				}
				return;
			}
		}
		foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
		{
			SimpleLockon.TarLockOn6m5s(allPlayer2, (info.CastTime - 5f) * 1000f);
		}
	}
}

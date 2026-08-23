using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M3S;

public class Diveboom : ISpecialAction
{
	public override string Name => "Diveboom (stack / spread)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37868u, 37869u, 37877u, 37878u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37868:
		case 37869:
			SimpleElement.ShowText("Spread soon");
			PlayerHelper.AllPlayers.ForEach(delegate(IGameObject player)
			{
				SimpleLockon.TarLockOn5m5s(player, 6300f);
			});
			break;
		case 37877:
		case 37878:
			SimpleElement.ShowText("2+2 stacks soon");
			PlayerHelper.Tank.ForEach(delegate(IGameObject player)
			{
				SimpleLockon.ShareLockon2(player, 6300f);
			});
			PlayerHelper.Healer.ForEach(delegate(IGameObject player)
			{
				SimpleLockon.ShareLockon2(player, 6300f);
			});
			break;
		}
	}
}

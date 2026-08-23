using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class MistralStackSpread : ISpecialAction
{
	public override string Name => "Mistral (stack / spread)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40155u, 40154u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 40155)
		{
			SimpleElement.ShowText("Spread soon");
			{
				foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
				{
					SimpleLockon.TarLockOn5m5s(allPlayer, 14000f);
				}
				return;
			}
		}
		if (info.ActionId != 40154)
		{
			return;
		}
		SimpleElement.ShowText("Stack soon");
		foreach (IGameObject item in PlayerHelper.Healer)
		{
			SimpleLockon.ShareLockon(item, 14000f);
		}
	}
}

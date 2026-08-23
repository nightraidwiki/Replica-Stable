using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M6S;

public class TasteOfThunderFire : ISpecialAction
{
	public override string Name => "Taste of Thunder/Fire";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42631u, 42633u, 42653u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 42653)
		{
			SimpleElement.Circle(info.Pos, 3f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42653u }
			});
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 42631)
		{
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				SimpleLockon.ShareLockon(item);
			}
			return;
		}
		if (info.ActionId != 42633)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			SimpleLockon.TarLockOn6m5s(allPlayer);
		}
	}
}

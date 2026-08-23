using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class CycleOfDeath : ISpecialAction
{
	public override string Name => "Cycle of Death";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40310u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			SimpleLockon.ShareLockon(item, 3000f);
		}
	}
}

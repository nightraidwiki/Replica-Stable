using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P5;

public class ChaosVortex : ISpecialAction
{
	public override string Name => "Chaos Vortex";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47934u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			SimpleLockon.TarLockOn5m5s(allPlayer);
		}
	}
}

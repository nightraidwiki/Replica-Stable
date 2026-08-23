using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7S;

public class ElectrogeneticForce : ISpecialAction
{
	public override string Name => "Electrogenetic Force";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 43340u, 43358u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			SimpleElement.Circle(allPlayer, 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42374u }
			});
		}
	}
}

using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class CycleOfDeathP4 : ISpecialAction
{
	public override string Name => "Cycle of Death";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40247u, 40302u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject()?.TargetObject;
		if (gameObject != null)
		{
			SimpleLockon.ShareLockon(gameObject);
		}
	}
}

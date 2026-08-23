using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class TenstrikeTrio : ISpecialAction
{
	public override string Name => "Tenstrike Trio";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40249u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject()?.TargetObject;
		if (gameObject != null)
		{
			SimpleLockon.ShareLockon(gameObject, 1000f);
		}
	}
}

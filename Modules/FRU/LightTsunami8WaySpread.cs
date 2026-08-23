using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class LightTsunami8WaySpread : ISpecialAction
{
	public override string Name => "Light Tsunami (8-way spread)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40189u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			IGameObject? source = info.SourceId.GameObject();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40188u }
			};
			SimpleElement.FanToTarget(source, allPlayer, 60f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}
}

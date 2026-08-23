using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.UnendingCoil;

public class MeteorStream : ISpecialAction
{
	public override string Name => "Meteor Stream";

	public override uint Phase => 2u;

	public override uint WeatherID => 21u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		if (actorID.GameObject().BaseId != 2007457 || p1 != 4 || p2 != 8)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			SimpleElement.Circle(allPlayer, 4f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 9920u },
				TargetHitCount = 5
			});
		}
	}
}

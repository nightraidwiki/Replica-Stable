using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UWU;

public class FeatherRain : ISpecialAction
{
	public override string Name => "Feather Rain";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11085u };

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		bool flag = id == 7738;
		if (flag)
		{
			flag = source.BaseId - 8722 <= 1;
		}
		if (!flag)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			SimpleElement.Circle(new Vector3(allPlayer.Position.X, allPlayer.Position.Y, allPlayer.Position.Z), 3f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 11085u }
			});
		}
	}
}

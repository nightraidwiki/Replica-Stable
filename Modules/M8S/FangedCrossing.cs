using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class FangedCrossing : ISpecialAction
{
	public override string Name => "Fanged Crossing";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 18221 && id == 4561)
		{
			SimpleElement.Cross(source, 21f, 3.5f, source.Rotation.Radians(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 41943u }
			});
		}
	}
}

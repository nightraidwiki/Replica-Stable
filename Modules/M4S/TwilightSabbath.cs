using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class TwilightSabbath : ISpecialAction
{
	public override string Name => "Twilight Sabbath";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 17323)
		{
			var (angle, num, flag) = id switch
			{
				4566u => (-90.Degrees(), 0, true), 
				4567u => (-90.Degrees(), 8300, false), 
				4568u => (90.Degrees(), 0, true), 
				4569u => (90.Degrees(), 8300, false), 
				_ => default((Angle, int, bool)), 
			};
			if (angle != default(Angle))
			{
				SimpleElement.Fan(source, 60f, 180, source.Rotation.Radians() + angle, 3000f, num, new HitCounter
				{
					ActionID = new HashSet<uint> { 38441u, 38442u },
					TargetHitCount = (flag ? 2 : 4)
				});
			}
		}
	}
}

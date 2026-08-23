using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UnendingCoil;

public class CycleOfDeath : ISpecialAction
{
	public override string Name => "Cycle of Death";

	public override uint Phase => 5u;

	public override uint WeatherID => 32u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9962u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId.GameObject(), 4f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 9962u, 9963u },
			TargetHitCount = 3 + base.NumCasts
		});
		base.NumCasts++;
	}
}

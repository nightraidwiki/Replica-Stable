using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P3;

public class CenterStage : ISpecialAction
{
	public override string Name => "Center Stage";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47854u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

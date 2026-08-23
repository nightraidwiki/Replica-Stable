using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P3N;

public class BirdOfPrey : ISpecialAction
{
	public override string Name => "Bird of Prey";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30723u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

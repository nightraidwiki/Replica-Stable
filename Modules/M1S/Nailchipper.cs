using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M1S;

public class Nailchipper : ISpecialAction
{
	public override string Name => "Nailchipper";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38022u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 5f, 3000f, (info.CastTime - 3f) * 1000f, info.ActionId);
	}
}

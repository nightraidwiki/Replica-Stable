using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40955u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.SourceId, 8f, info.CastTime * 1000f);
	}
}

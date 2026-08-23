using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M11S;

public class Explosion : ISpecialAction
{
	public override string Name => "Explosion";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46112u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

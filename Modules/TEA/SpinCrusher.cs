using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class SpinCrusher : ISpecialAction
{
	public override string Name => "Spin Crusher";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 19058u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 10f, 90);
	}
}

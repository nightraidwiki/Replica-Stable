using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zelenia;

public class PowerBreak : ISpecialAction
{
	public override string Name => "PowerBreak";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 43184u, 43185u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

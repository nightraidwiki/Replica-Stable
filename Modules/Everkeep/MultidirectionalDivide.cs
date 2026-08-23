using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Everkeep;

public class MultidirectionalDivide : ISpecialAction
{
	public override string Name => "Multidirectional Divide";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37795u, 37796u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37795)
		{
			SimpleElement.Cross(info, 30f, 4f, 5000f);
		}
		if (info.ActionId == 37796)
		{
			SimpleElement.Cross(info, 40f, 2f, 5000f);
		}
	}
}

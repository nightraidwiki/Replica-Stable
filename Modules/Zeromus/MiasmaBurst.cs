using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class MiasmaBurst : ISpecialAction
{
	public override string Name => "Miasma Burst";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35657u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info);
	}
}

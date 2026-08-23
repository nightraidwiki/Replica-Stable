using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class FlareSpine : ISpecialAction
{
	public override string Name => "Flare Spine";

	public override HashSet<uint> ActionID => new HashSet<uint> { 35683u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 60f, 5f);
	}
}

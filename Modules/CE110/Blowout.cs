using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE110;

public class Blowout : ISpecialAction
{
	public override string Name => "Blowout";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41397u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.KnockBack(info, 50f);
	}
}

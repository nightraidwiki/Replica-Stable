using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SpheneDarkEx;

public class DeathWarning : ISpecialAction
{
	public override string Name => "Death Warning";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44565u, 44566u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 100f, 6f);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CenoteJaJa;

public class GoldenSplash : ISpecialAction
{
	public override string Name => "Golden Splash";

	public override HashSet<uint> ActionID => new HashSet<uint> { 38267u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 180);
	}
}

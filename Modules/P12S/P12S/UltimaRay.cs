using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P12S.P12S;

public class UltimaRay : ISpecialAction
{
	public override string Name => "Ultima Ray";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33584u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 20f, 3f);
	}
}

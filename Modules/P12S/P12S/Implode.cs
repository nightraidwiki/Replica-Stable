using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P12S.P12S;

public class Implode : ISpecialAction
{
	public override string Name => "Implode";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33587u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 4f);
	}
}

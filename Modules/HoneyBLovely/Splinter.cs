using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.HoneyBLovely;

public class Splinter : ISpecialAction
{
	public override string Name => "Splinter";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37230u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info, 8f);
	}
}

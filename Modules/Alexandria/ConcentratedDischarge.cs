using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Alexandria;

public class ConcentratedDischarge : ISpecialAction
{
	public override string Name => "Concentrated Discharge";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36327u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 45f, 7.5f, 45f);
	}
}

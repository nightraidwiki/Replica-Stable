using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TenderValley;

public class HeavyNeedle : ISpecialAction
{
	public override string Name => "Heavy Needle";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37386u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 36f, 50);
	}
}

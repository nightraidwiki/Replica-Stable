using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE110;

public class ShadesCrossing : ISpecialAction
{
	public override string Name => "Blowout";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42035u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info);
	}
}

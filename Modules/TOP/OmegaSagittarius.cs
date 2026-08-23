using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class OmegaSagittarius : ISpecialAction
{
	public override string Name => "Omega Sagittarius";

	public override uint Phase => 2u;

	public override uint WeatherID => 78u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31539u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE106;

public class CrystallizedEnergy : ISpecialAction
{
	public override string Name => "CrystallizedEnergy";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42732u, 41758u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

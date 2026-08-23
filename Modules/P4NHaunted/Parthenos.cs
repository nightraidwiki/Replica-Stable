using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P4NHaunted;

public class Parthenos : ISpecialAction
{
	public override string Name => "Parthenos";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33496u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 60f, 8f, 60f);
	}
}

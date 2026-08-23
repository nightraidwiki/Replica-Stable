using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1NHaunted;

public class Blaze : ISpecialAction
{
	public override string Name => "Blaze";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33056u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 12f, 3000f, 0f, 33056u);
	}
}

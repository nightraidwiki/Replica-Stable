using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.A4;

public class Sacrament : ISpecialAction
{
	public override string Name => "Sacrament";

	public override HashSet<uint> ActionID => new HashSet<uint> { 6885u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info.SourceId, 60f, 8f, info.Facing, 3000f, 0f, 6886u);
	}
}

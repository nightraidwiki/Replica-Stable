using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M10S;

public class CrestPivot : ISpecialAction
{
	public override string Name => "Crest Pivot";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46547u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaEndlessCurrent : ISpecialAction
{
	public override string Name => "Mermaid Daria Endless Current";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45863u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineRectOffset(info, 8f, 2000f, 5, -4f);
	}
}

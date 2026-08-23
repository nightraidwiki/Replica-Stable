using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class Decapitation : ISpecialAction
{
	public override string Name => "Decapitation";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41063u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.SourceId, 40f, 240, info.Facing, 3000f, 0f, 41065u);
	}
}

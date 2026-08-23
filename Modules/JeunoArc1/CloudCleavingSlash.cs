using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class CloudCleavingSlash : ISpecialAction
{
	public override string Name => "Cloud-cleaving Slash";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41078u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(new Vector3(865f, -750f, -820f), 25f, 35f, 6600f);
	}
}

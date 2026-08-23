using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Vanguard;

public class TailSwingCone : ISpecialAction
{
	public override string Name => "Tail Swing (cone)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36592u, 36593u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.SourceId, 20f, 180, info.Facing, info.CastTime * 1000f);
	}
}

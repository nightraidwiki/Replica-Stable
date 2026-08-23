using System.Collections.Generic;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE107;

public class BedrockUplift : ISpecialAction
{
	public override string Name => "BedrockUplift";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37805u, 37807u };

	public override void OnActionCast(ActorCastInfo info)
	{
	}
}

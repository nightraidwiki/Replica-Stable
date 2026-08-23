using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuDeep;

public class FairyPellyFireGleam : ISpecialAction
{
	public override string Name => "Fairy Pelly Fire Gleam";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45499u, 47397u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info);
	}
}

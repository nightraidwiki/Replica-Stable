using System.Collections.Generic;
using Replica.Engine.Module;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuVc;

public class FairyPellyFireRing : ISpecialAction
{
	public override string Name => "Fairy Pelly Fire Ring";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 45448u };

	public override void OnActionCast(ActorCastInfo info)
	{
		AutoDrawModule.Run(info);
	}
}

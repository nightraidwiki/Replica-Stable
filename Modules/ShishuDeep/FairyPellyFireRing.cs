using System.Collections.Generic;
using Replica.Engine.Module;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.ShishuDeep;

public class FairyPellyFireRing : ISpecialAction
{
	public override string Name => "Fairy Pelly Fire Ring";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45447u };

	public override void OnActionCast(ActorCastInfo info)
	{
		AutoDrawModule.Run(info);
	}
}

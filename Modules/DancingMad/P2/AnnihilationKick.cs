using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingMad.P2;

public class AnnihilationKick : ISpecialAction
{
	public override string Name => "Annihilation Kick";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47836u, 47837u };

	public override void OnActionCast(ActorCastInfo info)
	{
		List<StaticVfx> list = ModuleUtil.GetSpecialAction<PastFutureEnding>()?.aoes;
		if (list != null && list.Count > 0)
		{
			list[0].Remove();
			list.RemoveAt(0);
		}
		SimpleElement.Fan(info.Pos, 100f, 180, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		});
	}
}

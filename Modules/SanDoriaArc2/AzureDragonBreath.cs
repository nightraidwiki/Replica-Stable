using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.SanDoriaArc2;

public class AzureDragonBreath : ISpecialAction
{
	public override string Name => "Azure Dragon Breath";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44416u, 44417u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info.TargetId, 60f, 180, info.Facing + ((info.ActionId == 44416) ? (-90) : 90).Degrees(), info.CastTime * 1000f);
	}
}

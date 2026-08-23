using System.Collections.Generic;
using Lumina.Excel.Sheets;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CE107;

public class HoppingMad : ISpecialAction
{
	public override string Name => "Hopping Mad";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37323u, 30872u, 30873u, 37041u };

	public override void OnActionCast(ActorCastInfo info)
	{
		byte effectRange = Svc.Data.GetExcelSheet<Action>().GetRow(info.ActionId).EffectRange;
		SimpleElement.Donut(info.SourceId.GameObject(), (int)effectRange, 60f, 3500f, info.CastTime * 1000f - 2000f);
	}
}

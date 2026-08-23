using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.E3;

public class StormCommand : ISpecialAction
{
	public override string Name => "Storm Command";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19516u, 19518u, 20067u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (!Svc.Objects.LocalPlayer.StatusList.Any((IStatus status) => status.StatusId - 2238 <= 1))
		{
			switch (info.ActionId)
			{
			case 19516:
			case 19518:
			{
				List<StaticVfx> list2 = aoes;
				uint sourceId2 = info.SourceId;
				Angle facing2 = info.Facing;
				float castTime2 = info.CastTime * 1000f;
				list2.Add(SimpleElement.Rectangle(sourceId2, 50f, 5f, 0f, null, facing2, castTime2));
				break;
			}
			case 20067:
			{
				List<StaticVfx> list = aoes;
				uint sourceId = info.SourceId;
				Angle facing = info.Facing;
				float castTime = info.CastTime * 1000f;
				list.Add(SimpleElement.Rectangle(sourceId, 25f, 5f, 0f, null, facing, castTime));
				break;
			}
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.E3;

public class NightRaidCommand : ISpecialAction
{
	public override string Name => "Night Raid Command";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19490u, 19491u, 19516u, 19517u, 19518u, 19521u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 19490:
		case 19516:
		case 19518:
			if (Svc.Objects.LocalPlayer.HasStatus(2238u))
			{
				List<StaticVfx> list2 = aoes;
				uint sourceId2 = info.SourceId;
				Angle facing2 = info.Facing;
				float castTime2 = info.CastTime * 1000f;
				list2.Add(SimpleElement.Rectangle(sourceId2, 50f, 5f, 0f, null, facing2, castTime2));
			}
			break;
		case 19491:
		case 19517:
		case 19521:
			if (Svc.Objects.LocalPlayer.HasStatus(2239u))
			{
				List<StaticVfx> list = aoes;
				uint sourceId = info.SourceId;
				Angle facing = info.Facing;
				float castTime = info.CastTime * 1000f;
				list.Add(SimpleElement.Rectangle(sourceId, 50f, 5f, 0f, null, facing, castTime));
			}
			break;
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

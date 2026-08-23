using System;
using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.FRU;

public class IceDonut : ISpecialAction
{
	public override string Name => "Ice (donut)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40279u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (aoes.Count == 0 || Svc.Objects.LocalPlayer == null)
			{
				return Array.Empty<StaticVfx>();
			}
			StaticVfx staticVfx = aoes.OrderBy((StaticVfx aoe) => aoe.Owner.DistanceSquaredToTarget(Svc.Objects.LocalPlayer)).FirstOrDefault();
			if (staticVfx == null)
			{
				return Array.Empty<StaticVfx>();
			}
			return new StaticVfx[1] { staticVfx };
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2462)
		{
			aoes.Add(SimpleElement.Donut(info.TargetID.GameObject(), 3f, 12f, 5000f, (info.Time - 5f) * 1000f));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

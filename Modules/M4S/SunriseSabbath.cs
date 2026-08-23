using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M4S;

public class SunriseSabbath : ISpecialAction
{
	public override string Name => "Sunrise Sabbath (bait)";

	public override uint Phase => 8u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38437u, 38438u, 39257u, 39258u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (aoes.Count == 0 || Svc.Objects.LocalPlayer == null)
			{
				return Array.Empty<StaticVfx>();
			}
			List<StaticVfx> list = new List<StaticVfx>();
			bool flag = Svc.Objects.LocalPlayer.HasStatus(4000u);
			bool flag2 = Svc.Objects.LocalPlayer.HasStatus(4001u);
			if ((flag | flag2) && (Svc.Objects.LocalPlayer.GetStatusRemainingTime(4000u, out var time) || Svc.Objects.LocalPlayer.GetStatusRemainingTime(4001u, out time)) && time < 15f)
			{
				if (Vector3.Distance(Svc.Objects.LocalPlayer.Position, new Vector3(100f, 0f, 165f)) < 12f)
				{
					if (flag)
					{
						foreach (StaticVfx aoe in aoes)
						{
							if (aoe.Owner.GetParam(2970u, out var param) && param == 757)
							{
								list.Add(aoe);
							}
						}
					}
					else if (flag2)
					{
						foreach (StaticVfx aoe2 in aoes)
						{
							if (aoe2.Owner.GetParam(2970u, out var param2) && param2 == 756)
							{
								list.Add(aoe2);
							}
						}
					}
				}
				else
				{
					foreach (StaticVfx item in aoes.OrderBy((StaticVfx aoe) => Vector3.Distance(Svc.Objects.LocalPlayer.Position, aoe.Owner.Position)).Take(2))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		bool flag = info.StatusID == 2970;
		if (flag)
		{
			flag = info.Stack - 756 <= 1;
		}
		if (flag)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = ((info.Stack == 756) ? "general02xf" : "general02pxf"),
				radiusX = 6f,
				radiusZ = 40f,
				drawOnObject = true,
				fixRotation = false,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
				}
			};
			aoes.Add(DrawManager.Draw(element, info.TargetID.GameObject()));
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

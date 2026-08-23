using System;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.UWU;

public class SearingWind : ISpecialAction
{
	public override string Name => "Searing Wind";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11099u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 14f,
			radiusZ = 14f,
			drawOnObject = true,
			destroyTime = 8000f,
			refColor = new Vector4(1f, 1f, 1f, 0.5f),
			refTargetColor = new Vector4(1f, 1f, 1f, 0.5f)
		};
		aoes.Add(DrawManager.Draw(element, info.TargetId.GameObject()));
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 1578)
		{
			return;
		}
		foreach (StaticVfx aoe in aoes)
		{
			if (aoe.Owner == info.TargetID.GameObject())
			{
				aoe.initTime = Environment.TickCount64;
				aoe.DrawTime = (long)(info.Time * 1000f);
				aoe.StatusCheck = new StatusCheck
				{
					CheckObject = aoe.Owner,
					Status = 1578u
				};
			}
		}
	}
}

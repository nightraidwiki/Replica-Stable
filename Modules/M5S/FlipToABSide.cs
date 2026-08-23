using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M5S;

public class FlipToABSide : ISpecialAction
{
	public bool isBside;

	public uint sourceId;

	public override string Name => "Flip To A/B-Side";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42880u, 42881u, 42798u, 42807u, 42817u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 42881)
		{
			sourceId = info.SourceId;
			isBside = true;
		}
		else if (info.ActionId == 42880)
		{
			sourceId = info.SourceId;
			isBside = false;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		if (actionId != 42798 && actionId != 42807 && actionId != 42817)
		{
			return;
		}
		if (!isBside)
		{
			IGameObject[] array = new IGameObject[3]
			{
				PlayerHelper.Tank.FirstOrDefault(),
				PlayerHelper.Healer.FirstOrDefault(),
				PlayerHelper.DPS.FirstOrDefault()
			};
			foreach (IGameObject target in array)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan045_1bpxf",
					radiusX = 40f,
					radiusZ = 40f,
					drawOnObject = true,
					target = target,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 42883u }
					}
				}, sourceId.GameObject());
			}
			return;
		}
		foreach (IGameObject item in PlayerHelper.Healer)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				radiusX = 4f,
				radiusZ = 50f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42884u }
				}
			}, sourceId.GameObject());
			SimpleLockon.ShareRect4s(item, sourceId.GameObject());
		}
	}
}

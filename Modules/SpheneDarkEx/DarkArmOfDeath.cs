using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class DarkArmOfDeath : ISpecialAction
{
	public override string Name => "Dark Arm of Death";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44553u, 44554u, 44612u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 44612:
			SimpleElement.Rectangle(info.Pos, 100f, 6f, 0f, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 44612u }
			});
			break;
		case 44553:
		case 44554:
		{
			int num = ((info.ActionId == 44553) ? 6 : (-6));
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = new Vector3(100 + num + ((num > 0) ? 2 : (-2)), 0f, 85f),
				drawOnObject = false,
				radiusX = 12f,
				radiusZ = 100f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 44612u }
				}
			});
			break;
		}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 44553 <= 1)
		{
			int num = ((info.ActionId == 44553) ? 6 : (-6));
			Vector3 pos = new Vector3(100 + num, 0f, 85f);
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 44555u }
			};
			SimpleElement.Rectangle(pos, 100f, 12f, 0f, default(Angle), 3000f, 0f, hitCounter);
		}
	}
}

using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class TailThrust : ISpecialAction
{
	public override string Name => "Tail Thrust (knockback)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38406u, 38407u, 38408u, 38409u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 38406 || actionId == 38408)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = new Vector3((info.ActionId == 38406) ? 90 : 110, 0f, 165f),
				drawOnObject = false,
				radiusX = 18f,
				radiusZ = 18f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u }
				}
			}, info.SourceId.GameObject());
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = new Vector3((info.ActionId == 38406) ? 110 : 90, 0f, 165f),
				drawOnObject = false,
				radiusX = 18f,
				radiusZ = 18f,
				delayDrawTime = 6100f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u },
					TargetHitCount = 2
				}
			}, info.SourceId.GameObject());
			return;
		}
		actionId = info.ActionId;
		if (actionId == 38407 || actionId == 38409)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0501_nockback_omen01d1",
				Position = new Vector3((info.ActionId == 38407) ? 90 : 110, 0f, 165f),
				drawOnObject = false,
				radiusX = 25f,
				radiusZ = 25f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u }
				}
			}, info.SourceId.GameObject());
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0501_nockback_omen01d1",
				Position = new Vector3((info.ActionId == 38407) ? 110 : 90, 0f, 165f),
				drawOnObject = false,
				radiusX = 25f,
				radiusZ = 25f,
				delayDrawTime = 6100f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38414u, 38415u },
					TargetHitCount = 2
				}
			}, info.SourceId.GameObject());
		}
	}
}

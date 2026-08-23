using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class PredaceousPounce : ISpecialAction
{
	private static readonly HashSet<uint> CastingCircle = new HashSet<uint> { 38027u, 38029u, 38031u, 38033u, 38035u, 39633u };

	private static readonly HashSet<uint> CastingRect = new HashSet<uint> { 38026u, 38028u, 38030u, 38032u, 38034u, 39632u };

	public override string Name => "Predaceous Pounce";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint item in CastingCircle)
			{
				hashSet.Add(item);
			}
			foreach (uint item2 in CastingRect)
			{
				hashSet.Add(item2);
			}
			return hashSet;
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (CastingCircle.Contains(info.ActionId))
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 11f,
				radiusZ = 11f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38025u }
				}
			};
			switch (info.ActionId)
			{
			case 38027:
				drawElement.hitCounter.ActionID = new HashSet<uint> { 39709u };
				break;
			case 38029:
				drawElement.hitCounter.TargetHitCount = 1;
				break;
			case 38031:
				drawElement.hitCounter.TargetHitCount = 2;
				break;
			case 38033:
				drawElement.hitCounter.TargetHitCount = 3;
				break;
			case 38035:
				drawElement.hitCounter.TargetHitCount = 4;
				break;
			case 39633:
				drawElement.hitCounter.TargetHitCount = 5;
				break;
			}
			DrawManager.Draw(drawElement, info.SourceId.GameObject());
		}
		else if (CastingRect.Contains(info.ActionId))
		{
			DrawElement drawElement2 = new DrawElement
			{
				drawAvfx = "general02xf",
				Position = info.SourceId.GameObject().Position,
				drawOnObject = false,
				radiusX = 3f,
				targetPosition = new Vector3(info.Pos.X, 0f, info.Pos.Z),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 39270u }
				}
			};
			switch (info.ActionId)
			{
			case 38026:
				drawElement2.hitCounter.ActionID = new HashSet<uint> { 39704u };
				break;
			case 38028:
				drawElement2.hitCounter.TargetHitCount = 1;
				break;
			case 38030:
				drawElement2.hitCounter.TargetHitCount = 2;
				break;
			case 38032:
				drawElement2.hitCounter.TargetHitCount = 3;
				break;
			case 38034:
				drawElement2.hitCounter.TargetHitCount = 4;
				break;
			case 39632:
				drawElement2.hitCounter.TargetHitCount = 5;
				break;
			}
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
		}
	}
}

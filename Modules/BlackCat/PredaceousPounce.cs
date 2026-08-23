using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BlackCat;

public class PredaceousPounce : ISpecialAction
{
	private static readonly HashSet<uint> CastingCircle = new HashSet<uint> { 37683u, 37685u, 37687u, 37689u, 37691u, 39631u };

	private static readonly HashSet<uint> CastingRect = new HashSet<uint> { 37682u, 37684u, 37686u, 37688u, 37690u, 39630u };

	public override string Name => "Predaceous Pounce";

	public override uint Phase => 1u;

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
					ActionID = new HashSet<uint> { 37681u }
				}
			};
			switch (info.ActionId)
			{
			case 37683:
				drawElement.hitCounter.ActionID = new HashSet<uint> { 39703u };
				break;
			case 37685:
				drawElement.hitCounter.TargetHitCount = 1;
				break;
			case 37687:
				drawElement.hitCounter.TargetHitCount = 2;
				break;
			case 37689:
				drawElement.hitCounter.TargetHitCount = 3;
				break;
			case 37691:
				drawElement.hitCounter.TargetHitCount = 4;
				break;
			case 39631:
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
					ActionID = new HashSet<uint> { 39268u }
				}
			};
			switch (info.ActionId)
			{
			case 37682:
				drawElement2.hitCounter.ActionID = new HashSet<uint> { 39702u };
				break;
			case 37684:
				drawElement2.hitCounter.TargetHitCount = 1;
				break;
			case 37686:
				drawElement2.hitCounter.TargetHitCount = 2;
				break;
			case 37688:
				drawElement2.hitCounter.TargetHitCount = 3;
				break;
			case 37690:
				drawElement2.hitCounter.TargetHitCount = 4;
				break;
			case 39630:
				drawElement2.hitCounter.TargetHitCount = 5;
				break;
			}
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
		}
	}
}

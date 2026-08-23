using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class HallowedWings : ISpecialAction
{
	public override string Name => "Hallowed Wings";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27939u, 27940u, 27942u, 27943u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc).ToList();
		if (gameObject != null)
		{
			switch (info.ActionId)
			{
			case 27939:
				LeftHalfCleave(gameObject);
				NearestTankbuster(gameObject);
				break;
			case 27940:
				LeftHalfCleave(gameObject);
				FarthestTankbuster(gameObject);
				break;
			case 27942:
				RightHalfCleave(gameObject);
				NearestTankbuster(gameObject);
				break;
			case 27943:
				RightHalfCleave(gameObject);
				FarthestTankbuster(gameObject);
				break;
			case 27941:
				break;
			}
		}
	}

	private static void LeftHalfCleave(IGameObject sourceObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 60f,
			radiusZ = 40f,
			refRotation = 90.Degrees(),
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 27941u }
			}
		}, sourceObject);
	}

	private static void RightHalfCleave(IGameObject sourceObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 60f,
			radiusZ = 40f,
			refRotation = -90.Degrees(),
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 27944u }
			}
		}, sourceObject);
	}

	private static void NearestTankbuster(IGameObject sourceObject)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc).ToList())
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 10f,
				radiusZ = 10f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = sourceObject,
					CheckType = 2,
					Count = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 27945u },
					TargetHitCount = 2
				}
			}, item);
		}
	}

	private static void FarthestTankbuster(IGameObject sourceObject)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc).ToList())
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 10f,
				radiusZ = 10f,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = sourceObject,
					CheckType = 3,
					Count = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 27945u },
					TargetHitCount = 2
				}
			}, item);
		}
	}
}

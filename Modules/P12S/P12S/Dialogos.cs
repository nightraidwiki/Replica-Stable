using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P12S.P12S;

public class Dialogos : ISpecialAction
{
	public override string Name => "Dialogos";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33534u, 33535u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject checkObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16171);
		switch (info.ActionId)
		{
		case 33534:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					distanceCheck = new DistanceCheck
					{
						CheckObject = checkObject,
						CheckType = 3
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 33538u }
					}
				}, allPlayer);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bpxf",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					distanceCheck = new DistanceCheck
					{
						CheckObject = checkObject,
						CheckType = 2
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 33538u }
					}
				}, allPlayer);
			}
			break;
		}
		case 33535:
		{
			foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					distanceCheck = new DistanceCheck
					{
						CheckObject = checkObject,
						CheckType = 2
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 33538u }
					}
				}, allPlayer2);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bpxf",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					distanceCheck = new DistanceCheck
					{
						CheckObject = checkObject,
						CheckType = 3
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 33538u }
					}
				}, allPlayer2);
			}
			break;
		}
		}
	}
}

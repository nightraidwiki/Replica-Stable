using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M3S;

public class ChainDeathmatch : ISpecialAction
{
	private IGameObject? origin;

	public override string Name => "Chain Deathmatch";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		39732u, 39733u, 39734u, 39735u, 39917u, 39918u, 39919u, 39920u, 39664u, 39665u,
		39666u, 39667u
	};

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4019 && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.Channeling,
				drawAvfx = "chn_x6r3_judge0e1",
				drawOnObject = true,
				destroyTime = info.Time * 1000f,
				target = info.SourceID.GameObject(),
				StatusCheck = new StatusCheck
				{
					CheckObject = Svc.Objects.LocalPlayer,
					Status = 4019u
				}
			}, Svc.Objects.LocalPlayer, info.SourceID.GameObject());
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 274 && targetId == Svc.Objects.LocalPlayer.GameObjectId)
		{
			origin = actorId.GameObject();
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 30f,
			radiusZ = 25f,
			refOffsetZ = 5f,
			drawOnObject = true,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		};
		switch (info.ActionId)
		{
		case 39664:
		case 39666:
		case 39732:
		case 39734:
		case 39917:
		case 39919:
			drawElement.refRotation = info.Facing - 90.Degrees();
			break;
		case 39665:
		case 39667:
		case 39733:
		case 39735:
		case 39918:
		case 39920:
			drawElement.refRotation = info.Facing + 90.Degrees();
			break;
		}
		if (origin != null && origin.Position.AlmostEqual(info.SourceId.GameObject().Position, 5f))
		{
			drawElement.drawAvfx = "general02pxf";
			SimpleElement.ShowText("Take blue AoE, avoid orange");
			origin = null;
		}
		DrawManager.Draw(drawElement, info.SourceId.GameObject());
	}
}

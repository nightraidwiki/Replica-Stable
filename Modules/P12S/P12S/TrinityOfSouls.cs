using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class TrinityOfSouls : ISpecialAction
{
	private HashSet<uint> castAction = new HashSet<uint>
	{
		33505u, 33506u, 33507u, 33508u, 33509u, 33510u, 33511u, 33512u, 33513u, 33514u,
		33515u, 33516u
	};

	private bool invertMiddle;

	private IGameObject boss;

	private float rotation;

	public override string Name => "Trinity of Souls";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33505u, 33506u, 33511u, 33512u };

	public override void OnActionCast(ActorCastInfo info)
	{
		boss = info.SourceId.GameObject();
		rotation = boss.Rotation;
		switch (info.ActionId)
		{
		case 33505:
		case 33506:
			invertMiddle = false;
			break;
		case 33511:
		case 33512:
			invertMiddle = true;
			break;
		}
		SimpleElement.Fan(info.SourceId, 60f, 180, info.Facing, 3000f, 0f, info.ActionId);
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		switch (icon)
		{
		case 423u:
			AddSubsequentAOE(90.Degrees(), last: false);
			break;
		case 424u:
			AddSubsequentAOE(-90.Degrees(), last: false);
			break;
		case 431u:
			AddSubsequentAOE(90.Degrees(), last: true);
			break;
		case 432u:
			AddSubsequentAOE(-90.Degrees(), last: true);
			break;
		case 433u:
			AddSubsequentAOE(90.Degrees(), last: true);
			break;
		case 434u:
			AddSubsequentAOE(-90.Degrees(), last: true);
			break;
		}
	}

	private void AddSubsequentAOE(Angle offset, bool last)
	{
		Angle refRotation = ((!last && invertMiddle) ? (boss.Rotation.Radians() - offset) : (boss.Rotation.Radians() + offset));
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "gl_fan180_1bf",
			radiusX = 60f,
			radiusZ = 60f,
			refRotation = refRotation,
			fixRotation = true,
			drawOnObject = true,
			destroyTime = 2700f
		};
		DrawQueue.Enqueue((castAction, new(IGameObject, DrawElement[])[1] { (boss, new DrawElement[1] { drawElement }) }));
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BruteBomber;

public class LariatCombo : ISpecialAction
{
	public override string Name => "Lariat Combo";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39644u, 39645u, 39646u, 39647u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "general02xf",
			Position = info.SourceId.GameObject().Position,
			drawOnObject = false,
			radiusX = 30f,
			radiusZ = 25f,
			refOffsetZ = 5f,
			refRotation = info.Facing + 90.Degrees(),
			fixRotation = true
		};
		DrawElement drawElement2 = new DrawElement
		{
			drawAvfx = "general02xf",
			Position = info.SourceId.GameObject().Position,
			drawOnObject = false,
			radiusX = 30f,
			radiusZ = 25f,
			refOffsetZ = 5f,
			refRotation = info.Facing - 90.Degrees(),
			fixRotation = true
		};
		switch (info.ActionId)
		{
		case 39644:
			drawElement.destroyTime = 6000f;
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			drawElement2.delayDrawTime = 6000f;
			drawElement2.destroyTime = 4400f;
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
			break;
		case 39645:
			drawElement.destroyTime = 6000f;
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			drawElement.delayDrawTime = 6000f;
			drawElement.destroyTime = 4400f;
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		case 39646:
			drawElement2.destroyTime = 6000f;
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
			drawElement.delayDrawTime = 6000f;
			drawElement.destroyTime = 4400f;
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		case 39647:
			drawElement2.destroyTime = 6000f;
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
			drawElement2.delayDrawTime = 6000f;
			drawElement2.destroyTime = 4400f;
			DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer);
			break;
		}
	}
}

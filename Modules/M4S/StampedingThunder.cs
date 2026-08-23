using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class StampedingThunder : ISpecialAction
{
	public override string Name => "Stampeding Thunder";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38354u, 38355u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "general02xf",
			drawOnObject = false,
			radiusX = 15f,
			radiusZ = 40f,
			refRotation = info.Source.Rotation.Radians(),
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 36399u }
			}
		};
		switch (info.ActionId)
		{
		case 38354u:
			SimpleElement.ShowText("Go right →→→");
			drawElement.Position = new Vector3(info.Source.Position.X - 5f, 0f, info.Source.Position.Z);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		case 38355u:
			SimpleElement.ShowText("←←← Go left");
			drawElement.Position = new Vector3(info.Source.Position.X + 5f, 0f, info.Source.Position.Z);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		}
	}
}

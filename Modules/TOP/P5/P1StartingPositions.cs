using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P1StartingPositions : ISpecialAction
{
	public override string Name => "P1 (starting positions)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31624u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 31624)
		{
			return;
		}
		Plugin.DebugChat("P5 P1 guide init");
		new TimeHelper(4500L, delegate
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15724);
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				radiusX = 1.5f,
				radiusY = 5f,
				radiusZ = 1.5f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 32630u }
				}
			};
			switch (Svc.Objects.LocalPlayer?.GameObjectId.Mark() ?? HeaderMarkerEnum.None)
			{
			case HeaderMarkerEnum.Attack1:
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = 9.5f;
				drawElement.refOffsetZ = -10.3f;
				DrawManager.Draw(drawElement, target);
				drawElement.refOffsetX = -9.5f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack3:
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = 9.5f;
				drawElement.refOffsetZ = -5.8f;
				DrawManager.Draw(drawElement, target);
				drawElement.refOffsetX = -9.5f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.None:
			case HeaderMarkerEnum.Chain1:
			case HeaderMarkerEnum.Chain2:
			case HeaderMarkerEnum.Forbidden1:
			case HeaderMarkerEnum.Forbidden2:
				drawElement.refOffsetX = 7.26f;
				drawElement.refOffsetZ = -30.36f;
				DrawManager.Draw(drawElement, target);
				drawElement.refOffsetX = -6.66f;
				DrawManager.Draw(drawElement, target);
				drawElement.refOffsetX = -2.2f;
				drawElement.refOffsetZ = -33.34f;
				DrawManager.Draw(drawElement, target);
				drawElement.refOffsetX = 2.6f;
				drawElement.refOffsetZ = -33.38f;
				DrawManager.Draw(drawElement, target);
				break;
			}
			Plugin.DebugChat("P5 P1 starting positions");
		});
	}
}

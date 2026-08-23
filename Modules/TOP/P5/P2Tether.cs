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

namespace Replica.Modules.TOP.P5;

public class P2Tether : ISpecialAction
{
	public override string Name => "P2 (tether)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32788u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnHeaderMarkerChangeEvent(HeaderMarkerEnum headerMarker)
	{
		if (!base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		bool num = Svc.Objects.LocalPlayer.HasStatus(3427u);
		bool flag = Svc.Objects.LocalPlayer.HasStatus(3428u);
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15724);
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "share_trap01k1",
			radiusX = 2f,
			radiusY = 5f,
			radiusZ = 2f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 31604u }
			}
		};
		if (num)
		{
			switch (headerMarker)
			{
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = -4.88f;
				drawElement.refOffsetZ = -31.2f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = -10.14f;
				drawElement.refOffsetZ = -24.4f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack3:
				drawElement.refOffsetX = -10.3f;
				drawElement.refOffsetZ = -15.78f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = -4.5f;
				drawElement.refOffsetZ = -9.9f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = 5.1f;
				drawElement.refOffsetZ = -31.26f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain2:
				drawElement.refOffsetX = 10.18f;
				drawElement.refOffsetZ = -24.3f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain3:
				drawElement.refOffsetX = 10.28f;
				drawElement.refOffsetZ = -15.68f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Circle:
				drawElement.refOffsetX = 4.44f;
				drawElement.refOffsetZ = -9.7f;
				DrawManager.Draw(drawElement, target);
				break;
			}
		}
		if (flag)
		{
			switch (headerMarker)
			{
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = -7.8f;
				drawElement.refOffsetZ = -37.08f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = -16.18f;
				drawElement.refOffsetZ = -26.86f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack3:
				drawElement.refOffsetX = -17.18f;
				drawElement.refOffsetZ = -13.02f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = -8f;
				drawElement.refOffsetZ = -3.08f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = 7.7f;
				drawElement.refOffsetZ = -36.8f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain2:
				drawElement.refOffsetX = 16.12f;
				drawElement.refOffsetZ = -27.7f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Chain3:
				drawElement.refOffsetX = 17.04f;
				drawElement.refOffsetZ = -12.52f;
				DrawManager.Draw(drawElement, target);
				break;
			case HeaderMarkerEnum.Circle:
				drawElement.refOffsetX = 7.84f;
				drawElement.refOffsetZ = -2.8f;
				DrawManager.Draw(drawElement, target);
				break;
			}
		}
		Plugin.DebugChat("P5 P2 tether guide");
	}
}

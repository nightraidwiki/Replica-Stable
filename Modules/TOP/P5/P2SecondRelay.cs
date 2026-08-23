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

public class P2SecondRelay : ISpecialAction
{
	public override string Name => "P2 (second relay)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32788u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
	{
		IBattleChara battleChara = (IBattleChara)((source is IBattleChara) ? source : null);
		if (battleChara == null || battleChara.NameId != 7639 || !base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15720);
		HeaderMarkerEnum headerMarkerEnum = Svc.Objects.LocalPlayer?.GameObjectId.Mark() ?? HeaderMarkerEnum.None;
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "share_trap01k1",
			radiusX = 2f,
			radiusY = 5f,
			radiusZ = 2f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 33040u }
			}
		};
		switch (headerMarkerEnum)
		{
		case HeaderMarkerEnum.Chain2:
			drawElement.refOffsetX = 5f;
			drawElement.refOffsetZ = -25.24f;
			break;
		case HeaderMarkerEnum.Attack3:
			drawElement.refOffsetX = -3.8f;
			drawElement.refOffsetZ = -20.38f;
			break;
		case HeaderMarkerEnum.Attack4:
			drawElement.refOffsetX = -5.46f;
			drawElement.refOffsetZ = -27.22f;
			break;
		case HeaderMarkerEnum.Forbidden1:
			drawElement.refOffsetX = 11.02f;
			drawElement.refOffsetZ = 5.36f;
			break;
		case HeaderMarkerEnum.Forbidden2:
			drawElement.refOffsetX = -10.92f;
			drawElement.refOffsetZ = 5.36f;
			break;
		}
		switch (icon)
		{
		case 156u:
			switch (headerMarkerEnum)
			{
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = -18.48f;
				drawElement.refOffsetZ = -9.82f;
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = 18.44f;
				drawElement.refOffsetZ = -9.82f;
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = 10.26f;
				drawElement.refOffsetZ = -9.88f;
				break;
			}
			break;
		case 157u:
			switch (headerMarkerEnum)
			{
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = 18.44f;
				drawElement.refOffsetZ = -9.82f;
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = -18.48f;
				drawElement.refOffsetZ = -9.82f;
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = -9.4f;
				drawElement.refOffsetZ = -9.82f;
				break;
			}
			break;
		}
		DrawManager.Draw(drawElement, target);
		Plugin.DebugChat("P5 P2 second relay guide");
	}
}

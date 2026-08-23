using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P1CleavePositions : ISpecialAction
{
	public override string Name => "P1 (cleave positions)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31636u, 31637u };

	public override void OnActionCast(ActorCastInfo info)
	{
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
				ActionID = new HashSet<uint> { info.ActionId }
			}
		};
		if (info.ActionId == 31636)
		{
			switch (headerMarkerEnum)
			{
			case HeaderMarkerEnum.None:
			case HeaderMarkerEnum.Forbidden1:
			case HeaderMarkerEnum.Forbidden2:
				drawElement.refOffsetX = -15.34f;
				drawElement.refOffsetZ = -30.48f;
				break;
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = -2.98f;
				drawElement.refOffsetZ = -1.82f;
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = -10.32f;
				drawElement.refOffsetZ = -35.74f;
				break;
			case HeaderMarkerEnum.Attack3:
				drawElement.refOffsetX = -8.58f;
				drawElement.refOffsetZ = -9.2f;
				break;
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = -13.44f;
				drawElement.refOffsetZ = -7.02f;
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = -18.76f;
				drawElement.refOffsetZ = -19.68f;
				break;
			case HeaderMarkerEnum.Chain2:
				drawElement.refOffsetX = -8.78f;
				drawElement.refOffsetZ = -20.06f;
				break;
			}
			DrawManager.Draw(drawElement, info.SourceId.GameObject());
		}
		else
		{
			switch (headerMarkerEnum)
			{
			case HeaderMarkerEnum.None:
				drawElement.refOffsetX = 15.78f;
				drawElement.refOffsetZ = -30.48f;
				break;
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = 1.86f;
				drawElement.refOffsetZ = -1.04f;
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = 10.42f;
				drawElement.refOffsetZ = -35.28f;
				break;
			case HeaderMarkerEnum.Attack3:
				drawElement.refOffsetX = 8.78f;
				drawElement.refOffsetZ = -10.34f;
				break;
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = 15.66f;
				drawElement.refOffsetZ = -8.94f;
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = 18.04f;
				drawElement.refOffsetZ = -19.42f;
				break;
			case HeaderMarkerEnum.Chain2:
				drawElement.refOffsetX = 6.72f;
				drawElement.refOffsetZ = -20.06f;
				break;
			}
			DrawManager.Draw(drawElement, info.SourceId.GameObject());
		}
		Plugin.DebugChat("P5 P1 cleave position guide");
	}
}

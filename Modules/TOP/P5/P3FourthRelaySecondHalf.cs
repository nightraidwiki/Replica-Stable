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

public class P3FourthRelaySecondHalf : ISpecialAction
{
	public override string Name => "P3 (fourth relay, second half)";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32789u, 32374u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 32789)
		{
			base.CanDraw = true;
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 32374 || !base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		new TimeHelper(1000L, delegate
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15724);
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
					ActionID = new HashSet<uint> { 32374u }
				}
			};
			switch (headerMarkerEnum)
			{
			case HeaderMarkerEnum.Forbidden1:
				drawElement.refOffsetX = 10.42f;
				drawElement.refOffsetZ = -4.12f;
				break;
			case HeaderMarkerEnum.Forbidden2:
				drawElement.refOffsetX = -10.7f;
				drawElement.refOffsetZ = -4.42f;
				break;
			case HeaderMarkerEnum.Attack1:
				drawElement.refOffsetX = 18.9f;
				drawElement.refOffsetZ = -21.5f;
				break;
			case HeaderMarkerEnum.Attack2:
				drawElement.refOffsetX = -18.98f;
				drawElement.refOffsetZ = -20.54f;
				break;
			case HeaderMarkerEnum.Attack3:
				drawElement.refOffsetX = -2.74f;
				drawElement.refOffsetZ = -31.5f;
				break;
			case HeaderMarkerEnum.Attack4:
				drawElement.refOffsetX = -5.22f;
				drawElement.refOffsetZ = -37.66f;
				break;
			case HeaderMarkerEnum.Chain1:
				drawElement.refOffsetX = 9.86f;
				drawElement.refOffsetZ = -21.68f;
				break;
			case HeaderMarkerEnum.Chain2:
				drawElement.refOffsetX = 5.98f;
				drawElement.refOffsetZ = -37.24f;
				break;
			}
			DrawManager.Draw(drawElement, target);
			Plugin.DebugChat("P5 P3 fourth relay positions");
		});
	}
}

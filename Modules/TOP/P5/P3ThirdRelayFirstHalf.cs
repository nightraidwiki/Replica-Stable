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

namespace Replica.Modules.TOP.P5;

public class P3ThirdRelayFirstHalf : ISpecialAction
{
	public override string Name => "P3 (third relay, first half)";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32789u, 31638u, 31639u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 32789)
		{
			base.CanDraw = true;
			Plugin.DebugChat("P5 P3 guide init");
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((uint)(info.ActionId - 31638) > 1u || !base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		new TimeHelper(1000L, delegate
		{
			IGameObject target = info.SourceId.GameObject();
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
			if (info.ActionId == 31638)
			{
				switch (headerMarkerEnum)
				{
				case HeaderMarkerEnum.Forbidden1:
					drawElement.refOffsetX = 8.96f;
					drawElement.refOffsetZ = -9.3f;
					break;
				case HeaderMarkerEnum.Forbidden2:
					drawElement.refOffsetX = 9.46f;
					drawElement.refOffsetZ = 9.26f;
					break;
				case HeaderMarkerEnum.Attack1:
					drawElement.refOffsetX = -2.58f;
					drawElement.refOffsetZ = -18.08f;
					break;
				case HeaderMarkerEnum.Attack2:
					drawElement.refOffsetX = -2.4f;
					drawElement.refOffsetZ = 18.34f;
					break;
				case HeaderMarkerEnum.Attack3:
					drawElement.refOffsetX = -10.66f;
					drawElement.refOffsetZ = 1.76f;
					break;
				case HeaderMarkerEnum.Attack4:
					drawElement.refOffsetX = -17.44f;
					drawElement.refOffsetZ = 3.7f;
					break;
				case HeaderMarkerEnum.Chain1:
					drawElement.refOffsetX = -2.14f;
					drawElement.refOffsetZ = -8.84f;
					break;
				case HeaderMarkerEnum.Chain2:
					drawElement.refOffsetX = -16.88f;
					drawElement.refOffsetZ = -6.06f;
					break;
				}
				DrawManager.Draw(drawElement, target);
			}
			else
			{
				switch (headerMarkerEnum)
				{
				case HeaderMarkerEnum.Forbidden1:
					drawElement.refOffsetX = -9.24f;
					drawElement.refOffsetZ = 9.26f;
					break;
				case HeaderMarkerEnum.Forbidden2:
					drawElement.refOffsetX = -9.24f;
					drawElement.refOffsetZ = -9.02f;
					break;
				case HeaderMarkerEnum.Attack1:
					drawElement.refOffsetX = 2.6f;
					drawElement.refOffsetZ = 18.4f;
					break;
				case HeaderMarkerEnum.Attack2:
					drawElement.refOffsetX = 2.66f;
					drawElement.refOffsetZ = -18.14f;
					break;
				case HeaderMarkerEnum.Attack3:
					drawElement.refOffsetX = 11.76f;
					drawElement.refOffsetZ = -2.88f;
					break;
				case HeaderMarkerEnum.Attack4:
					drawElement.refOffsetX = 17.7f;
					drawElement.refOffsetZ = -4.68f;
					break;
				case HeaderMarkerEnum.Chain1:
					drawElement.refOffsetX = 2.6f;
					drawElement.refOffsetZ = 8.7f;
					break;
				case HeaderMarkerEnum.Chain2:
					drawElement.refOffsetX = 17.28f;
					drawElement.refOffsetZ = 5.6f;
					break;
				}
			}
			DrawManager.Draw(drawElement, target);
			Plugin.DebugChat("P5 P3 third relay positions");
		});
	}
}

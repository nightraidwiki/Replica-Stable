using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class P1OmegaCleave : ISpecialAction
{
	public override string Name => "P1 Omega (cleave)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31636u, 31637u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 210);
		DrawElement element = new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 4f,
			radiusZ = 4f,
			drawOnObject = true,
			delayDrawTime = (int)((info.CastTime - 3f) * 1000f),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		};
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o.GameObjectId != Svc.Objects.LocalPlayer.GameObjectId))
		{
			HeaderMarkerEnum headerMarkerEnum = item.GameObjectId.Mark();
			if ((uint)headerMarkerEnum <= 3u)
			{
				DrawManager.Draw(element, item);
			}
		}
	}
}

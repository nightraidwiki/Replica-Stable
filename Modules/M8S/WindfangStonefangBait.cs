using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class WindfangStonefangBait : ISpecialAction
{
	public override string Name => "Wind/Stone fang Bait";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41904u, 41887u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 41904)
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				if (allPlayer != Svc.Objects.LocalPlayer)
				{
					DrawManager.Draw(new DrawElement
					{
						drawAvfx = "gl_fan030_1bf",
						Position = info.Pos,
						drawOnObject = false,
						radiusX = 40f,
						radiusZ = 40f,
						target = allPlayer,
						hitCounter = new HitCounter
						{
							ActionID = new HashSet<uint> { 41905u }
						}
					});
				}
			}
			return;
		}
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan030_1bpf",
			Position = info.Pos,
			drawOnObject = false,
			radiusX = 40f,
			radiusZ = 40f,
			target = Svc.Objects.LocalPlayer,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 41888u }
			}
		});
	}
}

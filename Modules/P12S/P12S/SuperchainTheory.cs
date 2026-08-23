using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.Memory;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class SuperchainTheory : ISpecialAction
{
	public override string Name => "Superchain Theory I";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 2056 || info.Stack != 583)
		{
			return;
		}
		IEnumerable<TetherInfo> enumerable = Data.TetherPlayer.Where((TetherInfo x) => x.To == info.TargetID);
		IGameObject obj = info.TargetID.GameObject();
		foreach (TetherInfo item in enumerable)
		{
			float num = (obj.DistanceToTarget(item.From.GameObject()) / 3f + 1f) * 1000f;
			switch (item.TetherID)
			{
			case 228:
				SimpleElement.Circle(info.TargetID, 7f, Delay: Math.Max(num - 4000f, 0f), CastTime: Math.Min(num, 4000f));
				break;
			case 229:
				SimpleElement.Donut(info.TargetID, 6f, 70f, Delay: Math.Max(num - 4000f, 0f), CastTime: Math.Min(num, 4000f));
				break;
			case 230:
				foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
				{
					IGameObject? source = info.TargetID.GameObject();
					float delay = Math.Max(num - 4000f, 0f);
					float castTime = Math.Min(num, 4000f);
					SimpleElement.FanToTarget(source, allPlayer, 100f, 30, Follow: true, default(Angle), delay, castTime);
				}
				break;
			case 231:
				foreach (IGameObject item2 in PlayerHelper.Tank.Union(PlayerHelper.Healer))
				{
					DrawManager.Draw(new DrawElement
					{
						drawAvfx = "gl_fan030_1bpf",
						radiusX = 100f,
						radiusZ = 100f,
						drawOnObject = true,
						target = item2,
						destroyTime = Math.Min(num, 4000f),
						delayDrawTime = Math.Max(num - 4000f, 0f)
					}, info.TargetID.GameObject());
				}
				break;
			}
		}
	}
}

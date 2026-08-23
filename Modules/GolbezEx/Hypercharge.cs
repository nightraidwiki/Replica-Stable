using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.GolbezEx;

public class Hypercharge : ISpecialAction
{
	public bool IsGathering;

	public override string Name => "Hypercharge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45663u, 45664u, 45670u, 45696u, 45679u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 45663:
			IsGathering = true;
			break;
		case 45664:
			IsGathering = false;
			break;
		case 45679:
			SimpleElement.Rectangle(info);
			break;
		}
		ushort actionId = info.ActionId;
		if (actionId != 45670 && actionId != 45696)
		{
			return;
		}
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 30f,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				Angle = ((info.ActionId == 45670) ? 0.Degrees() : (-180.Degrees()))
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 45670u, 45696u }
			}
		}, Svc.Objects.LocalPlayer);
		if (IsGathering)
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn5m5s(allPlayer, 6200f);
			}
			return;
		}
		foreach (IGameObject item in PlayerHelper.Healer.Union(PlayerHelper.Tank))
		{
			SimpleLockon.Share5S(item, 6200f);
		}
	}
}

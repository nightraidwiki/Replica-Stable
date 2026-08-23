using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P1;

public class IdolTethers : ISpecialAction
{
	public override string Name => "Idol Tethers";

	public override HashSet<uint> ActionID => new HashSet<uint> { 47788u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 45)
		{
			IGameObject? gameObject = actorId.GameObject();
			if (gameObject.Position.Y == 18.5f && targetId == Svc.Objects.LocalPlayer.GameObjectId)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 10f,
					fixRotation = true,
					TetherCheck = new TetherCheck
					{
						CheckType = 1,
						TetherID = new HashSet<int> { 45 }
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47785u }
					}
				}, Svc.Objects.LocalPlayer);
			}
			if (gameObject.Position.Y == 7f)
			{
				SimpleLockon.TarLockOn5m5s(targetId.GameObject(), (base.NumCasts == 0) ? 5500 : 7500);
			}
			if (gameObject.Position.Y == 22.5f)
			{
				SimpleLockon.ShareLockon2(targetId.GameObject(), (base.NumCasts == 0) ? 1500 : 3500);
			}
		}
	}
}

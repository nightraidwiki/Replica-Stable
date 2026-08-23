using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingMad.P1;

public class ChainTrap : ISpecialAction
{
	private readonly Dictionary<StaticVfx, IGameObject> _knockbackSource = new Dictionary<StaticVfx, IGameObject>();

	public override string Name => "Chain Trap";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		foreach (StaticVfx aoe in aoes)
		{
			if (_knockbackSource.TryGetValue(aoe, out IGameObject value) && value != null)
			{
				if (aoe.KnockBackCheck != null)
				{
					aoe.KnockBackCheck.OriginPos = value.Position;
				}
				aoe.Enable = new WPos(localPlayer.Position).InCircle(new WPos(value.Position), 6f);
			}
			else
			{
				aoe.Enable = false;
			}
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 5078)
		{
			return;
		}
		IGameObject gameObject = info.TargetID.GameObject();
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		if (gameObject != null && localPlayer != null && SameRoleBucket(localPlayer, gameObject))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "nockback_omen04t1",
				radiusX = 6f,
				radiusZ = 6f,
				destroyTime = 5000f,
				delayDrawTime = (info.Time - 5f) * 1000f,
				StatusCheck = new StatusCheck
				{
					CheckObject = gameObject,
					Status = 5078u
				}
			}, gameObject);
			if (info.TargetID != localPlayer.GameObjectId)
			{
				StaticVfx staticVfx = DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 14f,
					drawOnObject = true,
					destroyTime = 5000f,
					delayDrawTime = (info.Time - 5f) * 1000f,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = gameObject.Position,
						Antiable = false
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47783u }
					}
				}, localPlayer);
				aoes.Add(staticVfx);
				_knockbackSource[staticVfx] = gameObject;
			}
		}
	}

	public override void Reset()
	{
		_knockbackSource.Clear();
		base.Reset();
	}

	private static bool SameRoleBucket(IGameObject a, IGameObject b)
	{
		if (!(a is IPlayerCharacter chara) || !(b is IPlayerCharacter chara2))
		{
			return false;
		}
		bool num = chara.GetRole() == CombatRole.DPS;
		bool flag = chara2.GetRole() == CombatRole.DPS;
		return num == flag;
	}
}

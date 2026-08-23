using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M9S;

public class SonicScreech : ISpecialAction
{
	private readonly uint[] buffs = new uint[8] { 4731u, 4732u, 4733u, 4734u, 4735u, 4736u, 4737u, 4738u };

	public override string Name => "Sonic Screech / Congregate";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45980u, 45981u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 45981)
		{
			IGameObject target = Svc.Objects.LocalPlayer;
			if (Svc.Objects.LocalPlayer.HasStatus(buffs))
			{
				target = PlayerHelper.AllPlayers.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
			}
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bpxf",
				radiusX = 40f,
				radiusZ = 40f,
				drawOnObject = true,
				target = target,
				destroyTime = info.CastTime * 1000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45983u }
				}
			}, info.SourceId.GameObject());
		}
		if (info.ActionId == 45980)
		{
			IGameObject gameObject = PlayerHelper.Tank.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
			IGameObject gameObject2 = PlayerHelper.Healer.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
			IGameObject gameObject3 = PlayerHelper.DPS.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
			switch (Svc.Objects.LocalPlayer.GetRole())
			{
			case CombatRole.Tank:
			{
				IGameObject? source7 = info.SourceId.GameObject();
				IGameObject target4 = (Svc.Objects.LocalPlayer.HasStatus(buffs) ? gameObject : Svc.Objects.LocalPlayer);
				HitCounter hitCounter3 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source7, target4, 40f, 100, Follow: true, default(Angle), 0f, 3000f, hitCounter3);
				IGameObject? source8 = info.SourceId.GameObject();
				hitCounter3 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source8, gameObject2, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter3);
				IGameObject? source9 = info.SourceId.GameObject();
				hitCounter3 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source9, gameObject3, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter3);
				break;
			}
			case CombatRole.Healer:
			{
				IGameObject? source4 = info.SourceId.GameObject();
				HitCounter hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source4, gameObject, 40f, 100, Follow: true, default(Angle), 0f, 3000f, hitCounter2);
				IGameObject? source5 = info.SourceId.GameObject();
				IGameObject target3 = (Svc.Objects.LocalPlayer.HasStatus(buffs) ? gameObject2 : Svc.Objects.LocalPlayer);
				hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source5, target3, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter2);
				IGameObject? source6 = info.SourceId.GameObject();
				hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source6, gameObject3, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter2);
				break;
			}
			case CombatRole.DPS:
			{
				IGameObject? source = info.SourceId.GameObject();
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source, gameObject, 40f, 100, Follow: true, default(Angle), 0f, 3000f, hitCounter);
				IGameObject? source2 = info.SourceId.GameObject();
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source2, gameObject2, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter);
				IGameObject? source3 = info.SourceId.GameObject();
				IGameObject target2 = (Svc.Objects.LocalPlayer.HasStatus(buffs) ? gameObject3 : Svc.Objects.LocalPlayer);
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45982u }
				};
				SimpleElement.FanToTarget(source3, target2, 40f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter);
				break;
			}
			}
		}
	}
}

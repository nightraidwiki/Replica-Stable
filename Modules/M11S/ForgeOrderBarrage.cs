using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M11S;

public class ForgeOrderBarrage : ISpecialAction
{
	public override string Name => "Forge Order: Barrage";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46114u, 46115u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 46114)
		{
			SimpleLockon.TankShare(PlayerHelper.Tank[0], 2500f);
			SimpleElement.Circle(PlayerHelper.Tank[0], 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46091u }
			});
			{
				foreach (IGameObject item in PlayerHelper.DPS.Union(PlayerHelper.Healer))
				{
					SimpleLockon.TarLockOn6m5s(item, 2500f);
				}
				return;
			}
		}
		foreach (IGameObject item2 in PlayerHelper.Tank)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				drawOnObject = true,
				radiusX = 60f,
				radiusZ = 60f,
				target = item2,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46095u }
				}
			}, info.Source);
		}
		IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
		IGameObject gameObject = ((localPlayer != null && localPlayer.GetRole() == CombatRole.Tank) ? PlayerHelper.Healer.First() : Svc.Objects.LocalPlayer);
		IGameObject target = gameObject;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan045_1bpxf",
			drawOnObject = true,
			target = target,
			radiusX = 60f,
			radiusZ = 60f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 46096u }
			}
		}, info.Source);
	}
}

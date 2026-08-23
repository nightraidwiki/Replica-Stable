using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

namespace Replica.Modules.DancingMad.P3;

public class RoleGrouping : ISpecialAction
{
	public override string Name => "Resounding Slap Role Grouping";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47846u, 47847u };

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		int num = ModuleUtil.GetSpecialAction<ResoundingSlap>()?.aoes.Count ?? 0;
		int num2 = ModuleUtil.GetSpecialAction<Implosion>()?.aoes.Count ?? 0;
		bool enable = num <= 2 && num2 == 0;
		foreach (StaticVfx aoe in aoes)
		{
			if (aoe != null)
			{
				aoe.Enable = enable;
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		CombatRole role = Svc.Objects.LocalPlayer.GetRole();
		if (info.ActionId == 47846)
		{
			DrawRoleCone(info.Pos, Svc.Objects.LocalPlayer, RoleColor(role), 47850u);
		}
		else if (info.ActionId == 47847)
		{
			DrawRoleCone(info.Pos, Svc.Objects.LocalPlayer, RoleColor(role), 47851u);
			if (role != CombatRole.Tank)
			{
				DrawRoleCone(info.Pos, PlayerHelper.Tank.FirstOrDefault(), RoleColor(CombatRole.Tank), 47851u);
			}
			if (role != CombatRole.Healer)
			{
				DrawRoleCone(info.Pos, PlayerHelper.Healer.FirstOrDefault(), RoleColor(CombatRole.Healer), 47851u);
			}
			if (role != CombatRole.DPS)
			{
				DrawRoleCone(info.Pos, PlayerHelper.DPS.FirstOrDefault(), RoleColor(CombatRole.DPS), 47851u);
			}
		}
	}

	private void DrawRoleCone(Vector3 pos, IGameObject? target, Vector4 color, uint hitAction)
	{
		if (target != null)
		{
			aoes.Add(DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customFan",
				refRadian = 60f.Degrees().Rad,
				Position = pos,
				Enable = false,
				drawOnObject = false,
				target = target,
				radiusX = 100f,
				radiusZ = 100f,
				refColor = color,
				refTargetColor = color,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { hitAction }
				}
			}));
		}
	}

	private static Vector4 RoleColor(CombatRole role)
	{
		float w = Plugin.Config.CustomAlpha * 0.5f;
		return role switch
		{
			CombatRole.Tank => new Vector4(0.15f, 0.45f, 1f, w), 
			CombatRole.Healer => new Vector4(0.2f, 0.85f, 0.35f, w), 
			_ => new Vector4(1f, 0.25f, 0.25f, w), 
		};
	}
}

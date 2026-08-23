using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class ElectronStream : ISpecialAction
{
	private const uint Positron = 4000u;

	private const uint Negatron = 4001u;

	private const uint Far = 4002u;

	private const uint Near = 4003u;

	private const uint Spinning = 4004u;

	private const uint RoundHouse = 4005u;

	private const uint Colider = 4006u;

	public override string Name => "Electron Stream";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38360u, 38361u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = ((info.ActionId == 38360) ? "general02xf" : "general02pxf"),
			radiusX = 5f,
			radiusZ = 40f,
			refRotation = info.Facing,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		}, info.SourceId.GameObject());
		if (Svc.Objects.LocalPlayer.HasStatus(4000u))
		{
			SimpleElement.ShowText("Take blue AoE");
		}
		else if (Svc.Objects.LocalPlayer.HasStatus(4001u))
		{
			SimpleElement.ShowText("Take yellow AoE");
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 38360)
		{
			return;
		}
		new TimeHelper(100L, delegate
		{
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 17322);
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "co_trap00h1",
				Position = gameObject.Position,
				drawOnObject = false,
				radiusX = 1f,
				radiusY = 5f,
				radiusZ = 1f,
				refOffsetX = 2f,
				refOffsetZ = -2f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38362u, 38363u, 38364u }
				}
			};
			DrawElement drawElement2 = new DrawElement
			{
				drawAvfx = "share_trap01k1",
				Position = gameObject.Position,
				drawOnObject = false,
				radiusX = 2f,
				radiusY = 5f,
				radiusZ = 2f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38362u, 38363u, 38364u }
				}
			};
			if (Svc.Objects.LocalPlayer.HasStatus(4004u) || Svc.Objects.LocalPlayer.HasStatus(4005u))
			{
				drawElement.refOffsetX = -2.5f;
				drawElement.refOffsetZ = 2.5f;
				DrawManager.Draw(drawElement, gameObject);
				drawElement.refOffsetX = -2.5f;
				drawElement.refOffsetZ = -2.5f;
				DrawManager.Draw(drawElement, gameObject);
				drawElement.refOffsetX = 2.5f;
				drawElement.refOffsetZ = 2.5f;
				DrawManager.Draw(drawElement, gameObject);
				drawElement.refOffsetX = 2.5f;
				drawElement.refOffsetZ = -2.5f;
				DrawManager.Draw(drawElement, gameObject);
			}
			else if (Svc.Objects.LocalPlayer.HasStatus(4002u) || Svc.Objects.LocalPlayer.HasStatus(4003u) || Svc.Objects.LocalPlayer.HasStatus(4006u))
			{
				drawElement2.refOffsetZ = 5f;
				DrawManager.Draw(drawElement2, gameObject);
				drawElement2.refOffsetZ = -5f;
				DrawManager.Draw(drawElement2, gameObject);
			}
			List<IBattleChara> list = PlayerHelper.AllPlayers.Cast<IBattleChara>().ToList();
			bool flag = list.Any((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4003));
			foreach (IBattleChara item in list.Where((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId - 4002 <= 1)).ToList())
			{
				foreach (IBattleChara item2 in list)
				{
					if (item != item2)
					{
						DrawManager.Draw(new DrawElement
						{
							drawAvfx = "customFan",
							refRadian = 15.Degrees().Rad,
							drawOnObject = true,
							target = item2,
							radiusX = 50f,
							radiusZ = 50f,
							refColor = GroundOmen.friendColor,
							refTargetColor = GroundOmen.friendColor,
							distanceCheck = new DistanceCheck
							{
								CheckObject = item,
								CheckType = ((!flag) ? 1 : 0)
							},
							hitCounter = new HitCounter
							{
								ActionID = new HashSet<uint> { 38362u }
							}
						}, item);
					}
				}
			}
			foreach (IBattleChara item3 in list.Where((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4004)).ToList())
			{
				SimpleElement.Circle(item3, 2f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 38363u }
				});
			}
			foreach (IBattleChara item4 in list.Where((IBattleChara x) => x.StatusList.Any((IStatus status) => status.StatusId == 4005)).ToList())
			{
				SimpleElement.Donut(item4, 10f, 25f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 38364u }
				});
			}
		});
	}
}

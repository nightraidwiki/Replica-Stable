using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M11S;

public class VeteranUltima : ISpecialAction
{
	private readonly Queue<List<(IGameObject?, DrawElement[])>> drawList = new Queue<List<(IGameObject, DrawElement[])>>();

	public override string Name => "Veteran Ultima";

	public override HashSet<uint> ActionID => new HashSet<uint> { 47085u, 47086u, 46104u, 46105u, 46106u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 47085)
		{
			drawList.Clear();
			base.CanDraw = true;
		}
		if (info.ActionId == 47086)
		{
			new TimeHelper(2000L, DrawAoe);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (base.CanDraw && info.ActionId - 47085 > 1)
		{
			DrawAoe();
		}
	}

	public void DrawAoe()
	{
		if (drawList.Count > 0)
		{
			foreach (var item2 in drawList.Dequeue())
			{
				DrawElement[] item = item2.Item2;
				for (int i = 0; i < item.Length; i++)
				{
					DrawManager.Draw(item[i], item2.Item1);
				}
			}
			return;
		}
		base.CanDraw = false;
	}

	public override void Reset()
	{
		drawList.Clear();
		base.Reset();
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (id - 4561 > 2)
		{
			return;
		}
		switch (source.BaseId)
		{
		case 19184u:
		{
			List<(IGameObject, DrawElement[])> list2 = new List<(IGameObject, DrawElement[])>();
			DrawElement drawElement5 = new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 8f,
				radiusZ = 8f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46104u }
				}
			};
			list2.Add((source, new DrawElement[1] { drawElement5 }));
			DrawElement drawElement6 = new DrawElement
			{
				drawAvfx = "com_share_4_5s_c0c",
				drawType = ElementType.LockOn
			};
			DrawElement drawElement7 = new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46107u }
				}
			};
			list2.Add((Svc.Objects.LocalPlayer, new DrawElement[2] { drawElement6, drawElement7 }));
			drawList.Enqueue(list2);
			break;
		}
		case 19185u:
		{
			List<(IGameObject, DrawElement[])> list3 = new List<(IGameObject, DrawElement[])>();
			DrawElement drawElement8 = new DrawElement
			{
				drawAvfx = "gl_sircle_6005at1",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46105u }
				}
			};
			DrawElement drawElement9 = new DrawElement
			{
				drawAvfx = "vfx/common/eff/ui_tgl002a_o.avfx",
				drawType = ElementType.Omen,
				drawOnObject = true,
				radiusX = 4.5f,
				radiusZ = 4.5f,
				refColor = new Vector4(0.6118f, 0.8627f, 0.898f, 1f),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46105u }
				}
			};
			list3.Add((source, new DrawElement[2] { drawElement8, drawElement9 }));
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				if (allPlayer != Svc.Objects.LocalPlayer)
				{
					DrawElement drawElement10 = new DrawElement
					{
						drawAvfx = "gl_fan030_1bf",
						radiusX = 60f,
						radiusZ = 60f,
						drawOnObject = true,
						target = allPlayer,
						hitCounter = new HitCounter
						{
							ActionID = new HashSet<uint> { 46108u }
						}
					};
					list3.Add((source, new DrawElement[1] { drawElement10 }));
				}
			}
			drawList.Enqueue(list3);
			break;
		}
		case 19186u:
		{
			List<(IGameObject, DrawElement[])> list = new List<(IGameObject, DrawElement[])>();
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "general_x02f",
				radiusX = 5f,
				radiusZ = 40f,
				drawOnObject = true,
				refRotation = source.Rotation.Radians(),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46106u }
				}
			};
			DrawElement drawElement2 = new DrawElement
			{
				drawAvfx = "general_x02f",
				radiusX = 5f,
				radiusZ = 40f,
				drawOnObject = true,
				refRotation = source.Rotation.Radians() + 90.Degrees(),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46106u }
				}
			};
			list.Add((source, new DrawElement[2] { drawElement, drawElement2 }));
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				DrawElement drawElement3 = new DrawElement
				{
					drawAvfx = "general02pxf",
					radiusX = 3f,
					radiusZ = 60f,
					drawOnObject = true,
					target = item,
					delayDrawTime = 0f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46109u }
					}
				};
				DrawElement drawElement4 = new DrawElement
				{
					drawAvfx = "ShareLazerGround5s",
					drawOnObject = true,
					target = item,
					refOffsetZ = -10f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46109u }
					}
				};
				list.Add((source, new DrawElement[2] { drawElement3, drawElement4 }));
			}
			drawList.Enqueue(list);
			break;
		}
		}
	}
}

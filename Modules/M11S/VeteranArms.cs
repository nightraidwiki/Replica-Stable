using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M11S;

public class VeteranArms : ISpecialAction
{
	public Dictionary<Vector3, IGameObject> WeaponPoint = new Dictionary<Vector3, IGameObject>();

	public List<uint> ActionList = new List<uint>();

	public override string Name => "Veteran Arms / Forged Assault";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46103u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 46103)
		{
			return;
		}
		WeaponPoint.Clear();
		ActionList.Clear();
		foreach (IGameObject item2 in Svc.Objects.Where(delegate(IGameObject x)
		{
			if (x.BaseId - 19184 <= 2)
			{
				ICharacter character = (ICharacter)((x is ICharacter) ? x : null);
				if (character != null)
				{
					return character.IsCharacterVisible();
				}
			}
			return false;
		}))
		{
			WeaponPoint.Add(item2.Position, item2);
		}
		List<KeyValuePair<Vector3, IGameObject>> list = SortClockwise(WeaponPoint, new Vector3(100f, 0f, 100f), 360f - (info.Facing.Deg + 182f));
		Queue<List<(IGameObject, DrawElement[])>> queue = new Queue<List<(IGameObject, DrawElement[])>>();
		foreach (KeyValuePair<Vector3, IGameObject> item3 in list)
		{
			switch (item3.Value.BaseId)
			{
			case 19184u:
			{
				List<(IGameObject, DrawElement[])> list3 = new List<(IGameObject, DrawElement[])>();
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
				list3.Add((item3.Value, new DrawElement[1] { drawElement5 }));
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
				list3.Add((Svc.Objects.LocalPlayer, new DrawElement[2] { drawElement6, drawElement7 }));
				queue.Enqueue(list3);
				ActionList.Add(46104u);
				break;
			}
			case 19185u:
			{
				List<(IGameObject, DrawElement[])> list4 = new List<(IGameObject, DrawElement[])>();
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
				list4.Add((item3.Value, new DrawElement[2] { drawElement8, drawElement9 }));
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
						list4.Add((item3.Value, new DrawElement[1] { drawElement10 }));
					}
				}
				queue.Enqueue(list4);
				ActionList.Add(46105u);
				break;
			}
			case 19186u:
			{
				List<(IGameObject, DrawElement[])> list2 = new List<(IGameObject, DrawElement[])>();
				DrawElement drawElement = new DrawElement
				{
					drawAvfx = "general_x02f",
					radiusX = 5f,
					radiusZ = 40f,
					drawOnObject = true,
					refRotation = item3.Value.Rotation.Radians(),
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
					refRotation = item3.Value.Rotation.Radians() + 90.Degrees(),
					fixRotation = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46106u }
					}
				};
				list2.Add((item3.Value, new DrawElement[2] { drawElement, drawElement2 }));
				foreach (IGameObject item4 in PlayerHelper.Healer)
				{
					DrawElement drawElement3 = new DrawElement
					{
						drawAvfx = "general02pxf",
						radiusX = 3f,
						radiusZ = 60f,
						drawOnObject = true,
						target = item4,
						hitCounter = new HitCounter
						{
							ActionID = new HashSet<uint> { 46109u }
						}
					};
					DrawElement drawElement4 = new DrawElement
					{
						drawAvfx = "ShareLazerGround5s",
						drawOnObject = true,
						target = item4,
						refOffsetZ = -10f,
						hitCounter = new HitCounter
						{
							ActionID = new HashSet<uint> { 46109u }
						}
					};
					list2.Add((item3.Value, new DrawElement[2] { drawElement3, drawElement4 }));
				}
				queue.Enqueue(list2);
				ActionList.Add(46106u);
				break;
			}
			}
		}
		foreach (var item5 in queue.Dequeue())
		{
			DrawElement[] item = item5.Item2;
			for (int num = 0; num < item.Length; num++)
			{
				DrawManager.Draw(item[num], item5.Item1);
			}
		}
		DrawQueue.Enqueue((new HashSet<uint> { ActionList[0] }, queue.Dequeue().ToArray()));
		DrawQueue.Enqueue((new HashSet<uint> { ActionList[1] }, queue.Dequeue().ToArray()));
	}

	public override void Reset()
	{
		ActionList.Clear();
		WeaponPoint.Clear();
		base.Reset();
	}

	private static List<KeyValuePair<Vector3, IGameObject>> SortClockwise(Dictionary<Vector3, IGameObject> points, Vector3 center, float startRotationDeg)
	{
		return points.OrderBy((KeyValuePair<Vector3, IGameObject> p) => GetRelativeAngle(p.Key, center, startRotationDeg)).ToList();
	}

	private static float GetRelativeAngle(Vector3 point, Vector3 center, float startRotationDeg)
	{
		if (startRotationDeg < 0f)
		{
			startRotationDeg += 360f;
		}
		float y = point.X - center.X;
		float num = point.Z - center.Z;
		float num2 = MathF.Atan2(y, 0f - num) * 180f / (float)Math.PI;
		if (num2 < 0f)
		{
			num2 += 360f;
		}
		float num3 = num2 - startRotationDeg;
		if (num3 < 0f)
		{
			num3 += 360f;
		}
		return num3;
	}
}

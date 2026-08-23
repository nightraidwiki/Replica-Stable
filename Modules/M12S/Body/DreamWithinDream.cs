using System.Collections.Generic;
using System.Linq;
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

namespace Replica.Modules.M12S.Body;

public class DreamWithinDream : ISpecialAction
{
	public enum LineType
	{
		JumpLine,
		ShareLine,
		FanLine,
		CircleLine,
		NoneLine
	}

	public Queue<Vector3> mimicSpawnOrder = new Queue<Vector3>();

	public Dictionary<Vector3, IGameObject> mimicCellPairs = new Dictionary<Vector3, IGameObject>();

	public Dictionary<IGameObject, (ulong, LineType)> playerTetherMap = new Dictionary<IGameObject, (ulong, LineType)>();

	public Queue<IGameObject> humanoidSpawnOrder = new Queue<IGameObject>();

	public Dictionary<IGameObject, IGameObject> humanoidTetherMap = new Dictionary<IGameObject, IGameObject>();

	public List<(Vector3, Angle)> FanStorge = new List<(Vector3, Angle)>();

	public Vector3 AoeStorge = Vector3.Zero;

	public Dictionary<IGameObject, uint> humanoidStage2 = new Dictionary<IGameObject, uint>();

	public bool Enable;

	public int CurrentStage;

	public override string Name => "Dream Within a Dream (Fourth Fate)";

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		46345u, 46354u, 46353u, 48098u, 46360u, 46361u, 46365u, 46366u, 46351u, 46352u,
		48303u
	};

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 46345)
		{
			Reset();
			Enable = true;
		}
		if (info.ActionId == 46354 && CurrentStage == 1)
		{
			FanStorge.Add((info.Pos, info.Facing));
		}
		if (info.ActionId == 46353 && CurrentStage == 1)
		{
			AoeStorge = info.Pos;
		}
		ushort actionId = info.ActionId;
		if ((uint)(actionId - 46351) <= 1u || actionId == 48303)
		{
			if (CurrentStage == 1)
			{
				humanoidStage2.Add(info.SourceId.GameObject(), info.ActionId);
			}
			if (CurrentStage == 5)
			{
				if (info.ActionId == 46351)
				{
					FanStorge.Add((info.Pos, info.Facing + 90.Degrees()));
					FanStorge.Add((info.Pos, info.Facing - 90.Degrees()));
				}
				else if (info.ActionId == 46352)
				{
					FanStorge.Add((info.Pos, info.Facing));
					FanStorge.Add((info.Pos, info.Facing + 180.Degrees()));
				}
				else if (info.ActionId == 48303)
				{
					AoeStorge = info.Pos;
				}
			}
		}
		if (info.ActionId == 46363)
		{
			_ = humanoidStage2[info.SourceId.GameObject()];
			if (info.ActionId == 46351)
			{
				SimpleElement.Fan(info.Pos, 60f, 90, info.Facing + 90.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46358u }
				});
				SimpleElement.Fan(info.Pos, 60f, 90, info.Facing - 90.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46358u }
				});
			}
			else if (info.ActionId == 46352)
			{
				SimpleElement.Fan(info.Pos, 60f, 90, info.Facing, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46358u }
				});
				SimpleElement.Fan(info.Pos, 60f, 90, info.Facing + 180.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46358u }
				});
			}
		}
		if (info.ActionId != 48098)
		{
			return;
		}
		if (CurrentStage == 2)
		{
			foreach (var item in FanStorge)
			{
				SimpleElement.Fan(item.Item1, 60f, 90, item.Item2, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46358u }
				});
			}
			SimpleElement.Circle(AoeStorge, 10f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 48789u }
			});
			FanStorge.Clear();
		}
		if (CurrentStage != 6)
		{
			return;
		}
		foreach (var item2 in FanStorge)
		{
			SimpleElement.Fan(item2.Item1, 60f, 90, item2.Item2, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46358u }
			});
		}
		SimpleElement.Circle(AoeStorge, 10f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 48789u }
		});
		FanStorge.Clear();
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 48098)
		{
			CurrentStage++;
			if (CurrentStage == 4)
			{
				DrawHumenLineAoe();
				DrawHumenLineAoe();
			}
			if (CurrentStage == 6 || CurrentStage == 8)
			{
				DrawTimeRestart();
				DrawTimeRestart();
				DrawTimeRestart();
				DrawTimeRestart();
			}
		}
		if (info.ActionId == 48303 && CurrentStage == 5)
		{
			AoeStorge = info.Pos;
		}
		if (info.ActionId - 46360 <= 1 && CurrentStage == 4)
		{
			DrawHumenLineAoe();
		}
	}

	public void DrawTimeRestart()
	{
		Vector3 mimicCellPos = mimicSpawnOrder.Dequeue();
		IGameObject key = mimicCellPairs[mimicCellPos];
		LineType item = playerTetherMap[key].Item2;
		DrawElement element = new DrawElement();
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject x) => x.Position == mimicCellPos);
		switch (item)
		{
		case LineType.ShareLine:
			element = new DrawElement
			{
				drawAvfx = "com_share_4_5s_c0c",
				drawOnObject = true,
				drawType = ElementType.LockOn,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46361u }
				}
			};
			DrawManager.Draw(element, target);
			break;
		case LineType.CircleLine:
			element = new DrawElement
			{
				drawAvfx = "general_1bxf",
				drawOnObject = true,
				drawType = ElementType.Omen,
				radiusX = 20f,
				radiusZ = 20f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 48099u }
				}
			};
			break;
		}
		DrawManager.Draw(element, target);
	}

	public void DrawHumenLineAoe()
	{
		if (humanoidSpawnOrder.Count > 0)
		{
			IGameObject key = humanoidSpawnOrder.Dequeue();
			LineType item = playerTetherMap[humanoidTetherMap[key]].Item2;
			DrawElement element = new DrawElement();
			IGameObject target = humanoidTetherMap[key];
			switch (item)
			{
			case LineType.ShareLine:
				element = new DrawElement
				{
					drawAvfx = "com_share_4_5s_c0c",
					drawOnObject = true,
					drawType = ElementType.LockOn,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46361u }
					}
				};
				DrawManager.Draw(element, target);
				break;
			case LineType.FanLine:
				element = new DrawElement
				{
					drawAvfx = "gl_fan045_1bf",
					drawOnObject = true,
					drawType = ElementType.Omen,
					radiusX = 50f,
					radiusZ = 50f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47394u }
					}
				};
				break;
			case LineType.CircleLine:
				element = new DrawElement
				{
					drawAvfx = "general_1bxf",
					drawOnObject = true,
					drawType = ElementType.Omen,
					radiusX = 20f,
					radiusZ = 20f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46360u }
					}
				};
				break;
			}
			DrawManager.Draw(element, target);
		}
	}

	public override void Reset()
	{
		mimicSpawnOrder.Clear();
		mimicCellPairs.Clear();
		playerTetherMap.Clear();
		humanoidStage2.Clear();
		humanoidTetherMap.Clear();
		humanoidSpawnOrder.Clear();
		AoeStorge = Vector3.Zero;
		FanStorge.Clear();
		CurrentStage = 0;
		Enable = false;
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source != null)
		{
			if (source.BaseId == 19210 && id == 4562)
			{
				mimicSpawnOrder.Enqueue(source.Position);
			}
			if (source.BaseId == 19204 && id == 7750 && CurrentStage == 2)
			{
				humanoidSpawnOrder.Enqueue(source);
			}
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (actorId.GameObject().BaseId == 19210 && Id == 373)
		{
			mimicCellPairs.Add(actorId.GameObject().Position, targetId.GameObject());
		}
		uint baseId = actorId.GameObject().BaseId;
		if (baseId == 19202 || baseId == 19204)
		{
			switch (Id)
			{
			case 367u:
				playerTetherMap[targetId.GameObject()] = (actorId, LineType.FanLine);
				humanoidTetherMap[actorId.GameObject()] = targetId.GameObject();
				break;
			case 368u:
				playerTetherMap[targetId.GameObject()] = (actorId, LineType.CircleLine);
				humanoidTetherMap[actorId.GameObject()] = targetId.GameObject();
				break;
			case 369u:
				playerTetherMap[targetId.GameObject()] = (actorId, LineType.ShareLine);
				humanoidTetherMap[actorId.GameObject()] = targetId.GameObject();
				break;
			case 374u:
				playerTetherMap[targetId.GameObject()] = (actorId, LineType.JumpLine);
				humanoidTetherMap[actorId.GameObject()] = targetId.GameObject();
				break;
			}
		}
	}
}

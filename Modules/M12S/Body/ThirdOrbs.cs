using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Body;

public class ThirdOrbs : ISpecialAction
{
	public enum Type
	{
		Dount,
		Circle,
		Thunder,
		Fire
	}

	public Dictionary<IGameObject, Queue<Type>> OrbQueues = new Dictionary<IGameObject, Queue<Type>>();

	public override string Name => "Third Fate Orbs";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46335u, 46336u, 46332u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (KeyValuePair<IGameObject, Queue<Type>> orbQueue in OrbQueues)
		{
			Type type = orbQueue.Value.Dequeue();
			Type type2 = orbQueue.Value.Dequeue();
			DrawAoe(orbQueue.Key, type);
			DrawAoe(orbQueue.Key, type2);
		}
	}

	public void DrawAoe(IGameObject targetObject, Type type)
	{
		switch (type)
		{
		case Type.Dount:
			SimpleElement.Donut(targetObject, 5f, 60f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46338u }
			});
			break;
		case Type.Circle:
			SimpleElement.Circle(targetObject, 8f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46337u }
			});
			break;
		case Type.Thunder:
			SimpleElement.Fan(targetObject, 60f, 120, 0.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46339u }
			});
			SimpleElement.Fan(targetObject, 60f, 120, 180.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46339u }
			});
			break;
		case Type.Fire:
			SimpleElement.Fan(targetObject, 60f, 120, 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46340u }
			});
			SimpleElement.Fan(targetObject, 60f, 120, 270.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46340u }
			});
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 46335)
		{
			switch (info.Source.BaseId)
			{
			case 19207u:
				OrbQueues[info.Target].Enqueue(Type.Dount);
				break;
			case 19206u:
				OrbQueues[info.Target].Enqueue(Type.Circle);
				break;
			case 19208u:
				OrbQueues[info.Target].Enqueue(Type.Thunder);
				break;
			case 19209u:
				OrbQueues[info.Target].Enqueue(Type.Fire);
				break;
			}
		}
		if (info.ActionId != 46332)
		{
			return;
		}
		new TimeHelper(1000L, delegate
		{
			foreach (KeyValuePair<IGameObject, Queue<Type>> orbQueue in OrbQueues)
			{
				if (orbQueue.Value.Count > 0)
				{
					Type type = orbQueue.Value.Dequeue();
					DrawAoe(orbQueue.Key, type);
				}
				if (orbQueue.Value.Count > 0)
				{
					Type type2 = orbQueue.Value.Dequeue();
					DrawAoe(orbQueue.Key, type2);
				}
			}
		});
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 19205 && id == 9356)
		{
			OrbQueues.Add(source, new Queue<Type>());
		}
	}

	public override void Reset()
	{
		OrbQueues.Clear();
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class InertCrystal : ISpecialAction
{
	private bool aoesAdded;

	public override string Name => "Inert Crystal";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43926u, 43952u, 44810u, 44809u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (aoesAdded)
		{
			return;
		}
		IEnumerable<IGameObject> enumerable = Svc.Objects.Where((IGameObject o) => o.BaseId == 18663);
		IEnumerable<IGameObject> enumerable2 = Svc.Objects.Where((IGameObject o) => o.BaseId == 18662);
		foreach (IGameObject item in enumerable)
		{
			aoes.Add(SimpleElement.Circle(item, 12f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 44810u, 44809u },
				TargetHitCount = 12
			}));
		}
		foreach (IGameObject item2 in enumerable2)
		{
			aoes.Add(SimpleElement.Circle(item2, 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 44810u, 44809u },
				TargetHitCount = 12
			}));
		}
		aoesAdded = true;
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		IEnumerable<IGameObject> enumerable = Svc.Objects.Where((IGameObject o) => o.BaseId == 18663);
		IEnumerable<IGameObject> enumerable2 = Svc.Objects.Where((IGameObject o) => o.BaseId == 18662);
		if (aoesAdded || icon != 470 || !enumerable.Any() || !enumerable2.Any())
		{
			return;
		}
		foreach (IGameObject item in enumerable)
		{
			aoes.Add(SimpleElement.Circle(item, 12f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 44810u, 44809u },
				TargetHitCount = 12
			}));
		}
		foreach (IGameObject item2 in enumerable2)
		{
			aoes.Add(SimpleElement.Circle(item2, 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 44810u, 44809u },
				TargetHitCount = 12
			}));
		}
		aoesAdded = true;
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 44809 > 1)
		{
			return;
		}
		Vector3 position = info.Source.Position;
		for (int i = 0; i < aoes.Count; i++)
		{
			if (position.AlmostEqual(aoes[i].Position, 1f))
			{
				aoes[i].Remove();
				aoes.RemoveAt(i);
				if (aoes.Count == 0)
				{
					aoesAdded = false;
				}
				break;
			}
		}
	}

	public override void Reset()
	{
		aoesAdded = false;
		base.Reset();
	}
}

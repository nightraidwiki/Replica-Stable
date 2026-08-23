using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.AlexandriaDt;

public class EnforcementRay : ISpecialAction
{
	private readonly List<Vector3> startingpositions = new List<Vector3>(2);

	private Vector3 center;

	public override string Name => "Enforcement Ray";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42737u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			aoes.SortBy((StaticVfx x) => (x.Owner.Position - center).LengthSquared());
			return aoes.Take(2);
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 18319 && id == 4570)
		{
			List<StaticVfx> list = aoes;
			HitCounter hitCounter = new HitCounter
			{
				ActionID = ActionID,
				TargetHitCount = 3
			};
			list.AddRange(SimpleElement.Cross(source, 36f, 4.5f, default(Angle), 3000f, 0f, hitCounter));
		}
		if (source.BaseId == 18313 && id == 4565)
		{
			startingpositions.Add(source.Position);
			if (startingpositions.Count == 2)
			{
				Vector3 vector = startingpositions[0];
				Vector3 vector2 = startingpositions[1];
				center = new Vector3((vector.X + vector2.X) * 0.5f, -190f, (vector.Z + vector2.Z) * 0.5f);
			}
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 282)
		{
			return;
		}
		for (int i = 0; i < aoes.Count; i++)
		{
			if (aoes[i].Owner.GameObjectId == targetId)
			{
				aoes[i].Owner = actorId.GameObject();
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		aoes.Where((StaticVfx aoe) => aoe.Owner.Position.AlmostEqual(info.Source.Position, 1f)).ToList().ForEach(delegate(StaticVfx x)
		{
			x.Remove();
			aoes.Remove(x);
		});
	}
}

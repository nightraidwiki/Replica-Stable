using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M5S;

public class GetDownOutIn : ISpecialAction
{
	private static readonly HashSet<uint> CastEnd = new HashSet<uint> { 39908u, 42850u, 42851u };

	public override string Name => "GetDown! OutIn";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(39908u);
			foreach (uint item in CastEnd)
			{
				hashSet.Add(item);
			}
			return hashSet;
		}
	}

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 39908)
		{
			for (int i = 0; i < 4; i++)
			{
				aoes.Add(SimpleElement.Circle(info.SourceId.GameObject(), 7f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 39908u, 42850u, 42851u },
					TargetHitCount = 8
				}));
				aoes.Add(SimpleElement.Donut(info.SourceId.GameObject(), 5f, 40f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 39908u, 42850u, 42851u },
					TargetHitCount = 8
				}));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (CastEnd.Contains(info.ActionId) && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

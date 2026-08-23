using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M5S;

public class InsideOutOutsideIn : ISpecialAction
{
	private static readonly HashSet<uint> CastEnd = new HashSet<uint> { 37826u, 37827u, 37828u, 37829u };

	public override string Name => "Inside Out/Outside In";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(42876u);
			hashSet.Add(42878u);
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
		if (info.ActionId == 42876)
		{
			aoes.Add(SimpleElement.Circle(info.SourceId, 7f, 3000f, 0f, 37826u));
			aoes.Add(SimpleElement.Donut(info.SourceId, 5f, 40f, 3000f, 0f, 37827u, new Vector4(1f, 1f, 1f, 2f)));
		}
		else if (info.ActionId == 42878)
		{
			aoes.Add(SimpleElement.Donut(info.SourceId, 5f, 40f, 3000f, 0f, 37828u, new Vector4(1f, 1f, 1f, 2f)));
			aoes.Add(SimpleElement.Circle(info.SourceId, 7f, 3000f, 0f, 37829u));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (CastEnd.Contains(info.ActionId) && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

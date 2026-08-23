using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.ForkedTower;

public class RayofIgnorance : ISpecialAction
{
	public override string Name => "Ray of Ignorance";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41717u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info.Pos, 30f, 15f, 0f, info.Facing, 3000f, 0f, new HitCounter
		{
			ActionID = ActionID
		}));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

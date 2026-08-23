using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M9S;

public class AetherDrain : ISpecialAction
{
	public override string Name => "Aether Drain";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45970u, 45971u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(4);

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 45970:
			SimpleLockon.TarLockOn6m5s(info.TargetId.GameObject());
			break;
		case 45971:
			aoes.AddRange(SimpleElement.Cross(info.SourceId, 40f, 5f, info.Facing, info.CastTime * 1000f));
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 45971 && aoes.Count > 1)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

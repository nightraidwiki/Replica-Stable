using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.FRU;

public class TwinStillness : ISpecialAction
{
	public override string Name => "Twin Stillness / Silence";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40193u, 40194u, 40195u, 40196u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40193:
			aoes.Add(SimpleElement.Fan(info, 30f, 270));
			aoes.Add(SimpleElement.Fan(info.SourceId, 40f, 90, info.Facing + 180.Degrees(), 3000f, 0f, 40196u));
			break;
		case 40194:
			aoes.Add(SimpleElement.Fan(info, 40f, 90));
			aoes.Add(SimpleElement.Fan(info.SourceId, 30f, 270, info.Facing + 180.Degrees(), 3000f, 0f, 40195u));
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}

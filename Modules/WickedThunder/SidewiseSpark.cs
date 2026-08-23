using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.WickedThunder;

public class SidewiseSpark : ISpecialAction
{
	public override string Name => "Sidewise Spark";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37564u, 37565u, 37566u, 37567u, 39429u, 39439u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((uint)(info.ActionId - 37564) <= 3u)
		{
			aoes.Add(SimpleElement.Fan(info));
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 16996)
		{
			switch (id)
			{
			case 4568u:
				aoes.Add(SimpleElement.Fan(source.GameObjectId, 60f, 180, source.Rotation.Radians() + 90.Degrees(), 8000f));
				break;
			case 4566u:
				aoes.Add(SimpleElement.Fan(source.GameObjectId, 60f, 180, source.Rotation.Radians() - 90.Degrees(), 8000f));
				break;
			}
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

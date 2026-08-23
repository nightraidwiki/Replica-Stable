using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M8S;

public class FangedCharge : ISpecialAction
{
	public override string Name => "Fanged Charge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41942u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 18220 && id == 4562)
		{
			List<StaticVfx> list = aoes;
			Angle rotation = source.Rotation.Radians();
			list.Add(SimpleElement.Rectangle(source, 30f, 3f, 30f, null, rotation, 6000f));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

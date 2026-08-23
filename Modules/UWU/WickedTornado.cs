using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class WickedTornado : ISpecialAction
{
	public override string Name => "P4 Wicked Tornado";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11126u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 11126)
		{
			base.CanDraw = true;
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (base.CanDraw && source.BaseId == 8722 && id == 7747)
		{
			base.CanDraw = false;
			SimpleElement.Circle(source, 20f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 11087u }
			});
		}
	}
}

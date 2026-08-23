using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class P1EyeLine : ISpecialAction
{
	public override string Name => "P1 Eye Line";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31624u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		uint baseId = source.BaseId;
		if ((baseId == 14669 || baseId == 15724) && id == 7747 && base.CanDraw)
		{
			base.CanDraw = false;
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15716);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 8f,
				radiusZ = 100f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31521u }
				}
			}, target);
		}
	}
}

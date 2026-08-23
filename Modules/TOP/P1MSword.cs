using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class P1MSword : ISpecialAction
{
	public override string Name => "P1 M-Sword";

	public override uint Phase => 2u;

	public override uint WeatherID => 78u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31550u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 100 || !base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.BaseId - 15713 <= 1))
		{
			SimpleElement.Circle(item, 10f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 31526u }
			});
		}
	}
}

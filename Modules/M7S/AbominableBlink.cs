using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M7S;

public class AbominableBlink : ISpecialAction
{
	public override string Name => "Abominable Blink";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 327)
		{
			SimpleElement.Circle(target, 21f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 43156u }
			});
		}
	}
}

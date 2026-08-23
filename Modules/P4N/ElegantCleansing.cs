using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P4N;

public class ElegantCleansing : ISpecialAction
{
	public override string Name => "Elegant Cleansing";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 218)
		{
			SimpleElement.Circle(target, 5f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 27216u }
			});
		}
	}
}

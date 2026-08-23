using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.Ihuykatumu;

public class WindBlade : ISpecialAction
{
	public override string Name => "Wind Blade";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 506)
		{
			Angle rotation = 0.Degrees();
			SimpleElement.Rectangle(target, 36f, 4f, 36f, null, rotation, 5100f);
			rotation = 45.Degrees();
			SimpleElement.Rectangle(target, 36f, 4f, 36f, null, rotation, 5100f);
			rotation = 90.Degrees();
			SimpleElement.Rectangle(target, 36f, 4f, 36f, null, rotation, 5100f);
			rotation = 135.Degrees();
			SimpleElement.Rectangle(target, 36f, 4f, 36f, null, rotation, 5100f);
		}
	}
}

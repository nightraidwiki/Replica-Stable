using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M6S;

public class LightningStorm : ISpecialAction
{
	public override string Name => "Lightning Storm Lightning Storm";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 602)
		{
			SimpleLockon.TarLockOn8m5s(target, 4000f);
		}
	}
}

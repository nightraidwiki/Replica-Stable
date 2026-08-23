using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Valigarmanda;

public class ScourgeofFire : ISpecialAction
{
	public override string Name => "Scourge of Fire";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 508)
		{
			SimpleLockon.ShareLockon(target, 2000f);
		}
	}
}

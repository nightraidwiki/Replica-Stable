using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M2S;

public class PoisonSting : ISpecialAction
{
	public override string Name => "Poison Sting";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 234)
		{
			SimpleLockon.TarLockOn6m5s(target, 1000f);
		}
	}
}

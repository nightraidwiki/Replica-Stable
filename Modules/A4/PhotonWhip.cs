using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.A4;

public class PhotonWhip : ISpecialAction
{
	public override string Name => "Photon Whip";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 30)
		{
			SimpleElement.Circle(target, 4f);
		}
	}
}

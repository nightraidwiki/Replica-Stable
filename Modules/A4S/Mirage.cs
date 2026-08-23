using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.A4S;

public class Mirage : ISpecialAction
{
	public override string Name => "Mirage";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 8)
		{
			SimpleElement.Circle(target, 5f, 5000f);
		}
	}
}

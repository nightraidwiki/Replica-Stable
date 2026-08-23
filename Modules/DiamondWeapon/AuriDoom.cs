using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DiamondWeapon;

public class AuriDoom : ISpecialAction
{
	public override string Name => "Auri Doom";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 243)
		{
			SimpleElement.Circle(target, 5f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 24536u }
			});
		}
	}
}

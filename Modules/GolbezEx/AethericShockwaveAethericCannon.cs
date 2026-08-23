using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.GolbezEx;

public class AethericShockwaveAethericCannon : ISpecialAction
{
	public override string Name => "Aetheric Shockwave / Aetheric Cannon";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		switch (icon)
		{
		case 637u:
		{
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				SimpleLockon.ShareLockon2(item, 1500f);
			}
			break;
		}
		case 638u:
		{
			foreach (IGameObject item2 in PlayerHelper.DPS.Union(PlayerHelper.Healer))
			{
				SimpleLockon.TarLockOn6m5s(item2, 2500f);
			}
			break;
		}
		}
	}
}

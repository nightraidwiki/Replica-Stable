using System.Collections.Generic;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class MouserArenaReset : ISpecialAction
{
	public override string Name => "Mouser (arena reset)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnMapEffect(uint a2, ushort a3, ushort a4)
	{
		if (a3 == 4 && a4 == 8)
		{
			HookManager.ProcessMapEffectFunc(MapUtil.GetMapEffectModule(), a2, 4, 128);
		}
	}
}

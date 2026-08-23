using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TheMesoTerminal;

public class TheMesoTerminal : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 1028u
	};

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (2u, 108u) };
}

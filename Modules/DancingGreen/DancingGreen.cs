using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DancingGreen;

public class DancingGreen : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 1019u
	};

	public override string Author => "Null";

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (105u, 2u) };
}

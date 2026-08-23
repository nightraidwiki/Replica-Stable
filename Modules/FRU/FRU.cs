using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.FRU;

public class FRU : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 1006u
	};

	public override string Author => "Null";

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
	{
		(2u, 4u),
		(4u, 2u)
	};
}

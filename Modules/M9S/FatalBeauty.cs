using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M9S;

public class FatalBeauty : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 1069u
	};

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (2u, 105u) };

	public override string Author => "Null";
}

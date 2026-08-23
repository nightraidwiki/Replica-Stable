using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M4S;

public class WickedThunder : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 992u
	};

	public override string Author => "Null";

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (2u, 107u) };
}

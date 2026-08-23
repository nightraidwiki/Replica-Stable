using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.A4;

public class A4 : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 189u
	};

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (72u, 71u) };
}

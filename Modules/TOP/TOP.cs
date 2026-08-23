using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TOP;

public class TOP : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 908u
	};

	public override string Description => "P5 guides use party waymarks from your trigger system. Keep waymarks active through P5.";

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (78u, 79u) };
}

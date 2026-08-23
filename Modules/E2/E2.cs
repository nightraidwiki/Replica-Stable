using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.E2;

public class E2 : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 719u
	};

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 870u, 872u };
}

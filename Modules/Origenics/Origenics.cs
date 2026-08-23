using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Origenics;

public class Origenics : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 825u
	};
}

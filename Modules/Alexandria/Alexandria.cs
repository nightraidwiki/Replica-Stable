using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Alexandria;

public class Alexandria : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 827u
	};
}

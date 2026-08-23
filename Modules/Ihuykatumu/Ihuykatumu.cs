using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Ihuykatumu;

public class Ihuykatumu : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 826u
	};
}

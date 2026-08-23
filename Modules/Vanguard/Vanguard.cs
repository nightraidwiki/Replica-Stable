using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Vanguard;

public class Vanguard : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 831u
	};
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Praetorium;

public class Praetorium : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 16u
	};
}

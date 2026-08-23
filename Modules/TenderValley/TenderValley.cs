using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TenderValley;

public class TenderValley : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 834u
	};
}

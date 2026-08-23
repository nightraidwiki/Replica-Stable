using Replica.Engine.ModuleSetup;

namespace Replica.Modules.WorqorLarDor;

public class WorqorLarDor : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 824u
	};
}

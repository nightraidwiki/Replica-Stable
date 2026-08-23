using Replica.Engine.ModuleSetup;

namespace Replica.Modules.AlexandriaDt;

public class AlexandriaDt : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 1027u
	};
}

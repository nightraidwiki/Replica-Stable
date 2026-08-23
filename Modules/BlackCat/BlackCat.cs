using Replica.Engine.ModuleSetup;

namespace Replica.Modules.BlackCat;

public class BlackCat : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 985u
	};
}

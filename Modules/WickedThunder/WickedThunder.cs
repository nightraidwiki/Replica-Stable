using Replica.Engine.ModuleSetup;

namespace Replica.Modules.WickedThunder;

public class WickedThunder : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 991u
	};
}

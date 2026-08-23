using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P4N;

public class P4N : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 800u
	};
}

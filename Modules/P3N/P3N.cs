using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P3N;

public class P3N : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 876u
	};
}

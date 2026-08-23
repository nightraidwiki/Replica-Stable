using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P1N;

public class P1N : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 872u
	};
}

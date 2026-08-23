using Replica.Engine.ModuleSetup;

namespace Replica.Modules.BruteBomber;

public class BruteBomber : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 989u
	};
}

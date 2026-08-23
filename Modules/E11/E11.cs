using Replica.Engine.ModuleSetup;

namespace Replica.Modules.E11;

public class E11 : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 751u
	};
}

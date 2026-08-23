using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P4NHaunted;

public class P4NHaunted : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 942u
	};
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.E3;

public class E3 : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 726u
	};
}

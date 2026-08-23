using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Zeromus;

public class Zeromus : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 965u
	};
}

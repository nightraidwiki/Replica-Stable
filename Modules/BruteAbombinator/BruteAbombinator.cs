using Replica.Engine.ModuleSetup;

namespace Replica.Modules.BruteAbombinator;

public class BruteAbombinator : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 1023u
	};

	public override string Author => "Null";
}

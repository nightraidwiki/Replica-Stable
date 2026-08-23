using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Everkeep;

public class Everkeep : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 996u
	};

	public override string Author => "Null";
}

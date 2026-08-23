using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M1S;

public class BlackCat : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 986u
	};

	public override string Author => "Null";
}

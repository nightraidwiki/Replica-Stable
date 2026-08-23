using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Valigarmanda;

public class Valigarmanda : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 833u
	};

	public override string Author => "Null";
}

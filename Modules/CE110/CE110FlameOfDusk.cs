using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CE110;

public class CE110FlameOfDusk : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Foray,
		GroupType = GroupType.CriticalEngagement,
		GroupID = 1018u,
		NameID = 47u
	};

	public override string Author => "Null";
}

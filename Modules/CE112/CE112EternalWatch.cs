using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CE112;

public class CE112EternalWatch : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Foray,
		GroupType = GroupType.CriticalEngagement,
		GroupID = 1018u,
		NameID = 46u
	};

	public override string Author => "Null";
}

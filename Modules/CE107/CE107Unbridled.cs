using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CE107;

public class CE107Unbridled : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Foray,
		GroupType = GroupType.CriticalEngagement,
		GroupID = 1018u,
		NameID = 35u
	};

	public override string Author => "Null";
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CE103;

public class CE103WithExtremePrejudice : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Foray,
		GroupType = GroupType.CriticalEngagement,
		GroupID = 1018u,
		NameID = 43u
	};

	public override string Author => "Null";
}

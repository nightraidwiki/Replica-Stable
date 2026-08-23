using Replica.Engine.ModuleSetup;

namespace Replica.Modules.SpheneDarkEx;

public class SpheneDarkEx : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 1062u
	};
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UWU;

public class UWU : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 539u
	};
}

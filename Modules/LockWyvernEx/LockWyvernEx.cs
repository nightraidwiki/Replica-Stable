using Replica.Engine.ModuleSetup;

namespace Replica.Modules.LockWyvernEx;

public class LockWyvernEx : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 1044u
	};
}

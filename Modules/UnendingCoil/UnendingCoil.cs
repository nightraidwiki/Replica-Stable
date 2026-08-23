using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UnendingCoil;

public class UnendingCoil : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 280u
	};
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DiamondWeapon;

public class DiamondWeapon : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Trial,
		GroupType = GroupType.CFC,
		GroupID = 781u
	};
}

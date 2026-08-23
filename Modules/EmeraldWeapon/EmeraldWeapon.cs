using Replica.Engine.ModuleSetup;

namespace Replica.Modules.EmeraldWeapon;

public class EmeraldWeapon : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Trial,
		GroupType = GroupType.CFC,
		GroupID = 762u
	};

	public override bool UseAutoDraw => true;
}

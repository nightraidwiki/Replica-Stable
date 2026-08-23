using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UltimaWeapon;

public class UltimaWeapon : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Trial,
		GroupType = GroupType.CFC,
		GroupID = 830u
	};
}

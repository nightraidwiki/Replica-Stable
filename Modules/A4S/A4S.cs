using Replica.Engine.ModuleSetup;

namespace Replica.Modules.A4S;

public class A4S : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 139u
	};
}

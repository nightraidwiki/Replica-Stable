using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P1NHaunted;

public class P1NHaunted : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Raid,
		GroupType = GroupType.CFC,
		GroupID = 936u
	};
}

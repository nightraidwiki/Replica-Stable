using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CastrumMeridianum;

public class CastrumMeridianum : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 15u
	};
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.LunarSubterrane;

public class LunarSubterrane : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 823u
	};

	public override bool UseAutoDraw => true;
}

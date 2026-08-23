using Replica.Engine.ModuleSetup;

namespace Replica.Modules.SkydeepCenote;

public class SkydeepCenote : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Dungeon,
		GroupType = GroupType.CFC,
		GroupID = 829u
	};
}

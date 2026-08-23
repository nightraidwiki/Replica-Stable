using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CenoteJaJa;

public class CenoteJaJa : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.TreasureHunt,
		GroupType = GroupType.CFC,
		GroupID = 993u
	};
}

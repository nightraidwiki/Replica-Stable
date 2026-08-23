using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Manderville;

public class Manderville : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Trial,
		GroupType = GroupType.CFC,
		GroupID = 81u
	};

	public override bool UseAutoDraw => true;
}

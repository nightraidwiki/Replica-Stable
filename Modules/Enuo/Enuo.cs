using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Enuo;

public class Enuo : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 1116u
	};

	public override string Author => "Null";
}

using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DSR;

public class DSR : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 788u
	};

	public override string Author => "Null";
}

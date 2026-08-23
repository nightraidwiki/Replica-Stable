using Replica.Engine.ModuleSetup;

namespace Replica.Modules.SanDoriaArc2;

public class SanDoriaArc2 : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Alliance,
		GroupType = GroupType.CFC,
		GroupID = 1058u
	};

	public override string Author => "Null";
}

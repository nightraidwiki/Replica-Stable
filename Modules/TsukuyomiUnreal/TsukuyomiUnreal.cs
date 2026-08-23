using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TsukuyomiUnreal;

public class TsukuyomiUnreal : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Unreal,
		GroupType = GroupType.CFC,
		GroupID = 1067u
	};
}

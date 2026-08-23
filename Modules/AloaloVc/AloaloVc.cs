using Replica.Engine.ModuleSetup;

namespace Replica.Modules.AloaloVc;

public class AloaloVc : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.VariantCriterion,
		GroupType = GroupType.CFC,
		GroupID = 979u
	};

	public override string Author => "Null";
}

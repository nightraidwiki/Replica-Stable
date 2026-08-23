using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ShishuDeep;

public class ShishuDeep : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.VariantCriterion,
		GroupType = GroupType.CFC,
		GroupID = 1084u
	};

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 45544u };
}

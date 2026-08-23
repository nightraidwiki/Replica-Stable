using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ShishuVc;

public class ShishuVc : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.VariantCriterion,
		GroupType = GroupType.CFC,
		GroupID = 1079u
	};

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 45838u, 45128u, 45545u };

	public override string Author => "Null";
}

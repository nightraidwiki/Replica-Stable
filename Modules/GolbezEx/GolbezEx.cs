using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.GolbezEx;

public class GolbezEx : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Extreme,
		GroupType = GroupType.CFC,
		GroupID = 1077u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 45716u };
}

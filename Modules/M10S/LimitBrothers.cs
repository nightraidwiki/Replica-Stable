using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M10S;

public class LimitBrothers : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 1071u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 48639u, 48640u };
}

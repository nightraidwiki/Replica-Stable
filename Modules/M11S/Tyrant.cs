using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M11S;

public class Tyrant : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 1073u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 46085u };
}

using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M7S;

public class BruteAbombinator : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 1024u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 42330u, 43157u, 872u, 42262u };
}

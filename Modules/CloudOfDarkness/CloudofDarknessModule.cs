using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CloudOfDarkness;

public class CloudofDarknessModule : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Chaotic,
		GroupType = GroupType.CFC,
		GroupID = 1010u
	};

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 40441u, 40442u, 41348u, 41349u, 40534u };
}

using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ForkedTower;

public class TheForkedTower : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Foray,
		GroupType = GroupType.CriticalEngagement,
		GroupID = 1018u,
		NameID = 48u
	};

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 870u, 872u, 30415u, 30786u, 42040u, 42041u, 42043u, 42045u, 42981u, 42504u };

	public override bool DisableWeatherReset => true;
}

using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TEA;

public class TEA : DrawModule
{
	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 694u
	};

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
	{
		(108u, 132u),
		(132u, 108u)
	};

	public override string Description => "P3 time stop and P4 final verdict with position guides.\nP2 tethers: blue large circle = water stack, yellow = lightning stack.\nP4 Future's End α: small yellow circle = single target; blue or no circle = stack.";
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Enuo;

public class Vacuum : ISpecialAction
{
	public override string Name => "Vacuum";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		49995u, // SilentTorrentDash3
		49996u, // SilentTorrentDash
		49997u, // SilentTorrentDash2
		49998u, // SilentTorrentArc3
		49999u, // SilentTorrentArc1
		50000u, // SilentTorrentArc2
		50001u  // VacuumExplode
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 49995u || info.ActionId == 49996u || info.ActionId == 49997u || info.ActionId == 50001u)
		{
			SimpleElement.Circle(info, 7f);
		}
		else if (info.ActionId == 49998u || info.ActionId == 49999u || info.ActionId == 50000u)
		{
			SimpleElement.Donut(info, 17f, 19f);
		}
	}
}

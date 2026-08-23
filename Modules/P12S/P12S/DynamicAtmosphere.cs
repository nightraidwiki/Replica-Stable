using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P12S.P12S;

public class DynamicAtmosphere : ISpecialAction
{
	public override string Name => "Dynamic Atmosphere";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3591 && info.Time != 27f)
		{
			SimpleElement.Circle(info.TargetID, 7f, 3000f, 13000f, 33595u);
		}
	}
}

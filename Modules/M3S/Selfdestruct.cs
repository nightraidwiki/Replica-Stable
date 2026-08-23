using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M3S;

public class Selfdestruct : ISpecialAction
{
	public override string Name => "Fuses of Fury (self-destruct)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		switch (info.StatusID)
		{
		case 4017u:
			SimpleElement.Circle(info.TargetID, 8f, 5000f);
			break;
		case 4018u:
			SimpleElement.Circle(info.TargetID, 8f, 5000f, 5000f);
			break;
		}
	}
}

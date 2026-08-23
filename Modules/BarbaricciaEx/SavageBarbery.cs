using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BarbaricciaEx;

public class SavageBarbery : ISpecialAction
{
	public override string Name => "Savage Barbery";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30068u, 30069u, 30074u, 30075u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 30068:
			SimpleElement.Donut(info, 6f, 20f);
			break;
		case 30069:
		case 30075:
			SimpleElement.Circle(info);
			break;
		case 30074:
			SimpleElement.Rectangle(info, 20f, 6f, 20f);
			break;
		}
	}
}

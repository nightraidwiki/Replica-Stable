using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TsukuyomiUnreal;

public class Torment : ISpecialAction
{
	public override string Name => "Torment";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45359u, 45418u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.FanToTarget(info, 15f, 90);
	}
}

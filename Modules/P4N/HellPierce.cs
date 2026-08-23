using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P4N;

public class HellPierce : ISpecialAction
{
	public override string Name => "Hell Pierce";

	public override HashSet<uint> ActionID => new HashSet<uint> { 27215u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

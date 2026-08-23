using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.QueenEternalEx;

public class DivideAndConquer : ISpecialAction
{
	public override string Name => "Divide and Conquer";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30505u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

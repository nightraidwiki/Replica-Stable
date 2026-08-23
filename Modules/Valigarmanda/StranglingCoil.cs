using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class StranglingCoil : ISpecialAction
{
	public override string Name => "Strangling Coil";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36816u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(new Vector3(100f, 0f, 100f), 8f, 30f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 36816u }
		});
	}
}

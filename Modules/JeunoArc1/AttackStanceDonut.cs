using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class AttackStanceDonut : ISpecialAction
{
	public override string Name => "Attack Stance (donut)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40814u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(new Vector3(-500f, -500f, 600f), 16f, 30f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 40815u }
		});
	}
}

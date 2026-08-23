using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class AttackStanceIn : ISpecialAction
{
	public override string Name => "Attack Stance (in)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41116u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class AttackStanceCone : ISpecialAction
{
	public override string Name => "Attack Stance (cone)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41114u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

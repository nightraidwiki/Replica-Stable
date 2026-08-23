using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M7;

public class NeoBombarianSpecial : ISpecialAction
{
	public override string Name => "NeoBombarianSpecial";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42287u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.KnockBack(info, 90f, 1000f);
	}
}

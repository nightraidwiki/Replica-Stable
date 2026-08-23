using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class DisassemblyCombo : ISpecialAction
{
	public override string Name => "Disassembly Combo";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41084u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 40);
	}
}

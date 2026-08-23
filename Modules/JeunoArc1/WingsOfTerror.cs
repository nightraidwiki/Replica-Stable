using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class WingsOfTerror : ISpecialAction
{
	public override string Name => "Wings of Terror";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40848u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

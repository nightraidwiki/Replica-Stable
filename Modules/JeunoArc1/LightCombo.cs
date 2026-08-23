using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class LightCombo : ISpecialAction
{
	public override string Name => "Light Combo";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41085u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 4f, 40f);
	}
}

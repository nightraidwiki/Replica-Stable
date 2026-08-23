using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class DarkFireCity : ISpecialAction
{
	public override string Name => "Dark Fire (city)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40782u };

	public override uint Phase => 4u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 11.5f, 11.5f, 11.5f);
	}
}

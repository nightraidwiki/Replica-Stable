using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class LightPillar : ISpecialAction
{
	public override string Name => "Light Pillar";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11139u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 3f, 1000f);
	}
}

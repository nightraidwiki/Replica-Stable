using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CloudOfDarkness;

public class GhastlyGloomCross : ISpecialAction
{
	public override string Name => "Ghastly Gloom (cross)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40458u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Cross(info, 40f, 15f);
	}
}

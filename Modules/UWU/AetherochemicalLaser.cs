using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UWU;

public class AetherochemicalLaser : ISpecialAction
{
	public override string Name => "Aetherochemical Laser";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11140u, 11141u, 11142u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 46f, 4f, 6f);
	}
}

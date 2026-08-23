using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Origenics;

public class TelekinesisReact : ISpecialAction
{
	public override string Name => "Telekinesis React";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36428u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 70f, 6.5f);
	}
}

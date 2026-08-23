using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.A4S;

public class MegaBeam : ISpecialAction
{
	public override string Name => "Mega Beam";

	public override HashSet<uint> ActionID => new HashSet<uint> { 5938u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

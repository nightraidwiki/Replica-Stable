using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M12S.Body;

public class WingedCatastrophe : ISpecialAction
{
	public override string Name => "Winged Catastrophe";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46300u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

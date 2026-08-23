using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class CauterizeN : ISpecialAction
{
	public override string Name => "Cauterize";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27966u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info, 80f, 11f);
	}
}

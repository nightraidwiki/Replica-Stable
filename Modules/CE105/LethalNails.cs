using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CE105;

public class LethalNails : ISpecialAction
{
	public override string Name => "LethalNails";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41315u, 41316u, 41317u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(info);
	}
}

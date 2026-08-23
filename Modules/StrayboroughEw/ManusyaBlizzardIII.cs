using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.StrayboroughEw;

public class ManusyaBlizzardIII : ISpecialAction
{
	public override string Name => "Manusya Blizzard III";

	public override HashSet<uint> ActionID => new HashSet<uint> { 25238u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info);
	}
}

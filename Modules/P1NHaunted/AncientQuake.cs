using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.P1NHaunted;

public class AncientQuake : ISpecialAction
{
	public override string Name => "Ancient Quake";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33066u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.Pos, 8f, 7000f);
	}
}

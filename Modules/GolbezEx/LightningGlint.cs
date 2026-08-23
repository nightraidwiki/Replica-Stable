using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.GolbezEx;

public class LightningGlint : ISpecialAction
{
	public override string Name => "Lightning Glint";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45666u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info);
	}
}

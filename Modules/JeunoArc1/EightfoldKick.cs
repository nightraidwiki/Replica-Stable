using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class EightfoldKick : ISpecialAction
{
	public override string Name => "Eightfold Kick";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40957u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.TargetId, 6f, 3000f, 0f, 40957u);
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SanDoriaArc2;

public class CrimsonRiddle : ISpecialAction
{
	public override string Name => "Crimson Riddle";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45045u, 45044u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Fan(info, 180);
	}
}

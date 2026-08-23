using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.BlackCat;

public class One_twoPaw : ISpecialAction
{
	public override string Name => "One-two Paw";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37642u, 37643u, 37645u, 37646u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.CastTime < 7f)
		{
			SimpleElement.Fan(info, 60f, 180);
		}
		else
		{
			SimpleElement.Fan(info, 60f, 180, 6800f);
		}
	}
}

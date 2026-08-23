using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M8S;

public class WindfangStonefang : ISpecialAction
{
	public override string Name => "Wind/Stone fang";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41904u, 41885u, 41886u, 41889u, 41890u, 41887u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 41904:
			SimpleElement.Circle(info);
			break;
		case 41885:
		case 41886:
		case 41889:
		case 41890:
			SimpleElement.Cross(info);
			break;
		case 41887:
			SimpleElement.Donut(info);
			break;
		}
	}
}

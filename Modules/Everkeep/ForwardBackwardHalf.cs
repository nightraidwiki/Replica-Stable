using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Everkeep;

public class ForwardBackwardHalf : ISpecialAction
{
	public override string Name => "Forward / Backward Half";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37755u, 37756u, 37757u, 37758u, 39322u, 39323u, 39324u, 39325u };

	public override void OnActionCast(ActorCastInfo info)
	{
		(bool, bool) tuple;
		switch (info.ActionId)
		{
		case 37755:
		case 39322:
			tuple = (true, false);
			break;
		case 37756:
		case 39323:
			tuple = (true, true);
			break;
		case 37757:
		case 39324:
			tuple = (false, false);
			break;
		case 37758:
		case 39325:
			tuple = (false, true);
			break;
		default:
			tuple = default((bool, bool));
			break;
		}
		var (flag, flag2) = tuple;
		switch (info.ActionId)
		{
		case 37755:
		case 39322:
			SimpleElement.ShowText("Front-right safe");
			break;
		case 37756:
		case 39323:
			SimpleElement.ShowText("Front-left safe");
			break;
		case 37757:
		case 39324:
			SimpleElement.ShowText("Back-left safe");
			break;
		case 37758:
		case 39325:
			SimpleElement.ShowText("Back-right safe");
			break;
		}
		Angle angle = info.Facing + (flag ? 180 : 0).Degrees();
		IGameObject gameObject = info.SourceId.GameObject();
		Angle rotation = angle;
		HitCounter hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 37759u, 39282u, 37760u }
		};
		SimpleElement.Rectangle(gameObject, 50f, 30f, 10f, null, rotation, 3000f, 0f, hitCounter);
		IGameObject? gameObject2 = info.SourceId.GameObject();
		rotation = angle + (flag2 ? 90 : (-90)).Degrees();
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 37759u, 39282u, 37760u }
		};
		SimpleElement.Rectangle(gameObject2, 60f, 60f, 0f, null, rotation, 3000f, 0f, hitCounter);
	}
}

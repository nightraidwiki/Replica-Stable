using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class Tower : ISpecialAction
{
	public override string Name => "Tower";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40161u, 40162u, 40163u, 40164u, 40129u, 40130u, 40132u, 40133u, 40134u };

	public override void OnActionCast(ActorCastInfo info)
	{
		bool flag;
		switch (info.ActionId)
		{
		case 40129:
		case 40133:
		case 40161:
		case 40163:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			ActorCastInfo info2 = info;
			float delay = ((base.NumCasts == 0) ? ((info.CastTime - 3f) * 1000f) : 0f);
			SimpleElement.Rectangle(info2, 50f, 5f, 50f, null, delay);
			base.NumCasts++;
			return;
		}
		ushort actionId = info.ActionId;
		if (actionId == 40134 || actionId == 40164)
		{
			ActorCastInfo info3 = info;
			float delay2 = (info.CastTime - 3f) * 1000f;
			SimpleElement.Rectangle(info3, 50f, 10f, 50f, null, delay2);
		}
		else
		{
			SimpleElement.RectangleKnockBack2(info.SourceId.GameObject().Position, 50f, 30f, 50f, 0.Degrees(), 3000f, (info.CastTime - 2f) * 1000f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId },
				TargetHitCount = 1
			});
		}
	}
}

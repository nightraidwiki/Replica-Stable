using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class Geocentrism : ISpecialAction
{
	public override string Name => "Geocentrism";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33577u, 33578u, 33579u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 33577:
		{
			Vector3 pos = new Vector3(95f, 0f, 83f);
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			};
			SimpleElement.Rectangle(pos, 20f, 2f, 0f, default(Angle), 3000f, 0f, hitCounter);
			Vector3 pos2 = new Vector3(100f, 0f, 83f);
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			};
			SimpleElement.Rectangle(pos2, 20f, 2f, 0f, default(Angle), 3000f, 0f, hitCounter);
			Vector3 pos3 = new Vector3(105f, 0f, 83f);
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			};
			SimpleElement.Rectangle(pos3, 20f, 2f, 0f, default(Angle), 3000f, 0f, hitCounter);
			break;
		}
		case 33578:
			SimpleElement.Circle(new Vector3(100f, 0f, 90f), 2f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			SimpleElement.Donut(new Vector3(100f, 0f, 90f), 3f, 7f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			break;
		case 33579:
			SimpleElement.Rectangle(new Vector3(93f, 0f, 85f), 20f, 2f, 0f, 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			SimpleElement.Rectangle(new Vector3(93f, 0f, 90f), 20f, 2f, 0f, 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			SimpleElement.Rectangle(new Vector3(93f, 0f, 95f), 20f, 2f, 0f, 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			});
			break;
		}
	}
}

using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SkydeepCenote;

public class RockFist : ISpecialAction
{
	public override string Name => "Rock Fist";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36670u, 36671u, 36696u, 36697u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 36670 || actionId == 36696)
		{
			SimpleElement.Rectangle(info, 40f, 10f);
		}
		actionId = info.ActionId;
		if (actionId == 36671 || actionId == 36697)
		{
			ActionQueue.Enqueue((new HashSet<uint> { 36670u, 36696u }, delegate
			{
				SimpleElement.Rectangle(info, 40f, 10f);
			}));
		}
	}
}

using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class FlamesIceOfAscalon : ISpecialAction
{
	public override string Name => "Ice / Flames of Ascalon";

	public override uint Phase => 7u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 2056)
		{
			return;
		}
		IGameObject gameObject = info.TargetID.GameObject();
		if (gameObject.BaseId == 12616)
		{
			if (info.Stack == 298)
			{
				SimpleElement.Circle(gameObject, 8f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 28049u }
				});
			}
			else
			{
				SimpleElement.Donut(gameObject, 8f, 50f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 28050u }
				});
			}
		}
	}
}

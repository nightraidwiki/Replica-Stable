using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Util;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Enuo;

public class ChasingAndHoly : ISpecialAction
{
	public override string Name => "Chasing and Holy";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		48475u, // EndlessChaseCast
		49993u, // EndlessChaseInstant
		50044u, // DeepFreeze (spread 10f)
		50046u  // ShroudedHolyTargets (stack 6f)
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 48475u || info.ActionId == 49993u)
		{
			SimpleElement.Circle(info, 6f);
		}
		else if (info.ActionId == 50044u)
		{
			SimpleElement.Circle(info.TargetId, 10f, info.CastTime * 1000f, 0f, info.ActionId);
		}
		else if (info.ActionId == 50046u)
		{
			SimpleElement.Circle(info.TargetId, 6f, info.CastTime * 1000f, 0f, info.ActionId);
		}
	}

	public override void OnObjectCreatedEvent(IGameObject gameObject)
	{
		if (gameObject.BaseId == 20149) // NaughtHuntChaser
		{
			// Draw circle of 6f moving with the chaser orb
			SimpleElement.Circle(gameObject, 6f, 15000f);
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 404)
		{
			var src = actorId.GameObject();
			var trg = targetId.GameObject();
			if (src != null && trg != null)
			{
				// Draw a line helper connecting the orb and the player
				SimpleElement.RectangleToTarget(src, trg, 50f, 0.5f, 15000f);
			}
		}
	}
}

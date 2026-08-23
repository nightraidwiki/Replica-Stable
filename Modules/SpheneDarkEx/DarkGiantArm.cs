using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class DarkGiantArm : ISpecialAction
{
	public override string Name => "Dark Giant Arm";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 102)
		{
			IGameObject gameObject = actorId.GameObject();
			SimpleElement.Rectangle(gameObject.Position, 36f, 5f, 0f, gameObject.Rotation.Radians(), 12300f);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.CE103;

public class CommandUrn : ISpecialAction
{
	public override string Name => "CommandUrn";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == targetId);
		IGameObject gameObject2 = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == actorId);
		uint? num = gameObject?.BaseId;
		IGameObject gameObject3 = ((num.HasValue && num == 18146) ? gameObject : gameObject2);
		switch (Id)
		{
		case 306u:
			SimpleElement.Circle(gameObject3, 16f, 5000f);
			break;
		case 303u:
			SimpleElement.Circle(gameObject3, 16f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 41420u, 39470u }
			});
			break;
		case 304u:
		{
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 41421u, 39471u }
			};
			SimpleElement.Cross(gameObject3, 40f, 5f, default(Angle), 3000f, 0f, hitCounter);
			break;
		}
		case 305u:
			break;
		}
	}
}

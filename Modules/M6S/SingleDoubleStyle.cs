using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class SingleDoubleStyle : ISpecialAction
{
	public override string Name => "Single / Double Style";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id - 319 > 1)
		{
			return;
		}
		IGameObject gameObject = actorId.GameObject();
		if (gameObject != null)
		{
			switch (gameObject.BaseId)
			{
			case 18336u:
				SimpleElement.Circle(gameObject, 15f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42617u }
				});
				break;
			case 18337u:
				SimpleElement.Circle((new WPos(gameObject.Position) + 16f * gameObject.Rotation.Radians().ToDirection()).ToVec3(), 15f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42619u }
				});
				break;
			case 18338u:
			{
				Angle rotation = gameObject.Rotation.Radians();
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42630u }
				};
				SimpleElement.Rectangle(gameObject, 60f, 3.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
				break;
			}
			case 18340u:
				SimpleElement.Fan(gameObject, 50f, 100, gameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42628u }
				});
				break;
			case 18341u:
				SimpleElement.Circle(gameObject, 30f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42629u }
				});
				break;
			case 18339u:
				break;
			}
		}
	}
}

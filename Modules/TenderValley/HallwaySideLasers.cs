using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.TenderValley;

public class HallwaySideLasers : ISpecialAction
{
	private enum Type
	{
		Short,
		Medium,
		Long
	}

	private static readonly (Vector2 Position, Type Type)[] AOEMap = new(Vector2, Type)[4]
	{
		(new Vector2(-112.5f, -486.5f), Type.Medium),
		(new Vector2(-147.5f, -471.5f), Type.Medium),
		(new Vector2(-147.5f, -486.5f), Type.Short),
		(new Vector2(-112.5f, -471.5f), Type.Short)
	};

	public override string Name => "Hallway Side Lasers";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 3u;

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 37)
		{
			IGameObject gameObject = actorId.GameObject();
			switch (GetType(new Vector2(gameObject.Position.X, gameObject.Position.Z)) ?? Type.Long)
			{
			case Type.Short:
			{
				Angle rotation3 = gameObject.Rotation.Radians();
				HitCounter hitCounter3 = new HitCounter
				{
					ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
				};
				SimpleElement.Rectangle(gameObject, 12f, 4f, 0f, null, rotation3, 3000f, 0f, hitCounter3);
				break;
			}
			case Type.Medium:
			{
				Angle rotation2 = gameObject.Rotation.Radians();
				HitCounter hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
				};
				SimpleElement.Rectangle(gameObject, 22f, 4f, 0f, null, rotation2, 3000f, 0f, hitCounter2);
				break;
			}
			case Type.Long:
			{
				Angle rotation = gameObject.Rotation.Radians();
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 39823u, 39824u, 39825u }
				};
				SimpleElement.Rectangle(gameObject, 35f, 4f, 0f, null, rotation, 3000f, 0f, hitCounter);
				break;
			}
			}
		}
	}

	private static Type? GetType(Vector2 position)
	{
		(Vector2, Type)[] aOEMap = AOEMap;
		for (int i = 0; i < aOEMap.Length; i++)
		{
			var (pos, value) = aOEMap[i];
			if (position.AlmostEqual(pos, 1f))
			{
				return value;
			}
		}
		return null;
	}
}

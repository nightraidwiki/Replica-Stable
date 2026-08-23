using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Helper;

public class DistanceCheck
{
	public IGameObject? CheckObject;

	public Vector3 Position;

	public required int CheckType;

	public int Count = 1;
}

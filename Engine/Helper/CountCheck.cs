using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Helper;

public class CountCheck
{
	public required IGameObject CheckObject;

	public int Count;

	public float SafeAlpha = 0.4f;
}

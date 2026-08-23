using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Helper;

public class StatusCheck
{
	public required IGameObject CheckObject;

	public required uint Status;

	public bool Reverse;
}

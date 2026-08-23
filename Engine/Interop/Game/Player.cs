using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Interop.Game;

internal static class Player
{
	public static IGameObject Object => Plugin.ObjectTable.LocalPlayer;
}

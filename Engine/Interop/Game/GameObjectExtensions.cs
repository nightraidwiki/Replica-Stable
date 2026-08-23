using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Replica.Engine.Interop.Game;

internal static class GameObjectExtensions
{
	public unsafe static Character* Struct(this IGameObject obj)
	{
		return (Character*)obj.Address;
	}
}

using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Interop.Game;

internal static class CharacterFunctions
{
	public static int GetTransformationID(this ICharacter chara)
	{
		return 0;
	}

	public static bool IsHostile(this IGameObject obj)
	{
		if (obj is IBattleChara obj2)
		{
			return obj2.IsHostile();
		}
		return false;
	}
}

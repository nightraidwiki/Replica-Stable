using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;

namespace Replica.Engine.Helper;

public static class PlayerHelper
{
	public static List<IGameObject> AllPlayers => Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc).ToList();

	public static List<IGameObject> Tank => Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o is IPlayerCharacter chara && chara.GetRole() == CombatRole.Tank).ToList();

	public static List<IGameObject> Healer => Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o is IPlayerCharacter chara && chara.GetRole() == CombatRole.Healer).ToList();

	public static List<IGameObject> DPS => Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o is IPlayerCharacter chara && chara.GetRole() == CombatRole.DPS).ToList();

	public static float CameraDirHToCharaRotation(float cameraDirH)
	{
		return (cameraDirH - (float)Math.PI) % ((float)Math.PI * 2f);
	}

	public static IEnumerable<IGameObject> RaidByEnmity(ulong primaryTargetId, bool allowGuessing = true)
	{
		FightClientState.TargetEnmity currentTargetEnmity = FightClientState.CurrentTargetEnmity;
		FightClientState.EnmityEntry[] array = null;
		FightClientState.EnmityEntry[] entries;
		if (currentTargetEnmity.TargetId == primaryTargetId)
		{
			array = currentTargetEnmity.Entries;
		}
		else if (FightClientState.TryGetEnmity(primaryTargetId, out entries))
		{
			array = entries;
		}
		if (array != null)
		{
			List<IGameObject> list = (from h in array
				where h.EntityId != 0
				orderby h.Enmity descending
				select Svc.Objects.SearchById(h.EntityId) into o
				where o != null
				select o).Cast<IGameObject>().ToList();
			if (list.Count > 0)
			{
				return list;
			}
		}
		if (!allowGuessing)
		{
			return Array.Empty<IGameObject>();
		}
		ulong bossTargetId = (Svc.Objects.SearchById(primaryTargetId)?.TargetObject?.GameObjectId).GetValueOrDefault();
		return AllPlayers.OrderBy((IGameObject p) => GuessEnmityOrder(p, bossTargetId));
	}

	private static int GuessEnmityOrder(IGameObject player, ulong bossTargetId)
	{
		if (player.GameObjectId == bossTargetId)
		{
			return 0;
		}
		if (player is IPlayerCharacter chara)
		{
			return chara.GetRole() switch
			{
				CombatRole.Tank => 1, 
				CombatRole.Healer => 3, 
				_ => 2, 
			};
		}
		return 4;
	}
}

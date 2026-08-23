using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Util;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Interop;

namespace Replica.Modules.Enuo;

public class DenseAiryEmptiness : ISpecialAction
{
	public override string Name => "Dense and Airy Emptiness";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		50032u, // AiryEmptiness
		50033u  // DenseEmptiness
	};

	private List<IGameObject> GetHealers()
	{
		var healers = new List<IGameObject>();
		if (Plugin.PartyList.Length == 0)
		{
			var localPlayer = Plugin.ObjectTable.LocalPlayer;
			if (localPlayer != null && localPlayer.ClassJob.IsValid && localPlayer.ClassJob.Value.Role == 4)
			{
				healers.Add(localPlayer);
			}
		}
		else
		{
			foreach (var m in Plugin.PartyList)
			{
				var chara = Plugin.ObjectTable.SearchById(m.EntityId) as IBattleChara;
				if (chara != null && chara.ClassJob.IsValid && chara.ClassJob.Value.Role == 4)
				{
					healers.Add(chara);
				}
			}
		}
		return healers;
	}

	private List<IGameObject> GetTanks()
	{
		var tanks = new List<IGameObject>();
		if (Plugin.PartyList.Length == 0)
		{
			var localPlayer = Plugin.ObjectTable.LocalPlayer;
			if (localPlayer != null && localPlayer.ClassJob.IsValid && localPlayer.ClassJob.Value.Role == 1)
			{
				tanks.Add(localPlayer);
			}
		}
		else
		{
			foreach (var m in Plugin.PartyList)
			{
				var chara = Plugin.ObjectTable.SearchById(m.EntityId) as IBattleChara;
				if (chara != null && chara.ClassJob.IsValid && chara.ClassJob.Value.Role == 1)
				{
					tanks.Add(chara);
				}
			}
		}
		return tanks;
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		var src = info.SourceId.GameObject();
		if (src != null)
		{
			if (info.ActionId == 50033u) // Dense Emptiness (100 deg cone on Healers)
			{
				var healers = GetHealers();
				foreach (var trg in healers)
				{
					SimpleElement.FanToTarget(src, trg, 60f, 100, true, default, 0f, info.CastTime * 1000f);
				}
			}
			else if (info.ActionId == 50032u) // Airy Emptiness (60 deg cone on Tanks and Healers)
			{
				var targets = new List<IGameObject>();
				targets.AddRange(GetHealers());
				targets.AddRange(GetTanks());
				foreach (var trg in targets)
				{
					SimpleElement.FanToTarget(src, trg, 60f, 60, true, default, 0f, info.CastTime * 1000f);
				}
			}
		}
	}
}

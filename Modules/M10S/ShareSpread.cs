using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M10S;

public class ShareSpread : ISpecialAction
{
	public enum WaveType
	{
		None,
		Wave44,
		Wave8,
		Wave4,
		Wave1Share
	}

	public WaveType type;

	public override string Name => "Share / Spread";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46540u, 46547u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 46540)
		{
			return;
		}
		switch (type)
		{
		case WaveType.Wave44:
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				SimpleLockon.Share5S(item, 4500f);
			}
			break;
		case WaveType.Wave8:
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				SimpleLockon.TarLockOn6m5s(allPlayer, 4500f);
			}
			break;
		case WaveType.Wave4:
			foreach (IGameObject item2 in PlayerHelper.AllPlayers.Where((IGameObject x) => x.HasStatus(4975u)))
			{
				SimpleElement.Circle(item2, 5f, 3000f, 4500f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46543u, 46551u }
				});
			}
			break;
		case WaveType.Wave1Share:
			SimpleLockon.Share5S(PlayerHelper.AllPlayers.FirstOrDefault((IGameObject x) => x.HasStatus(4975u)), 4500f);
			break;
		}
		type = WaveType.None;
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 46547)
		{
			return;
		}
		switch (type)
		{
		case WaveType.Wave4:
			foreach (IGameObject item in PlayerHelper.AllPlayers.Where((IGameObject x) => x.HasStatus(4975u)))
			{
				SimpleElement.Circle(item, 5f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 46543u, 46551u }
				});
			}
			break;
		case WaveType.Wave1Share:
			SimpleLockon.Share5S(PlayerHelper.AllPlayers.FirstOrDefault((IGameObject x) => x.HasStatus(4975u)));
			break;
		}
		type = WaveType.None;
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2056 && info.TargetID.GameObject().BaseId == 19288)
		{
			type = info.Stack switch
			{
				1005u => WaveType.Wave44, 
				1006u => WaveType.Wave8, 
				1007u => WaveType.Wave1Share, 
				1008u => WaveType.Wave4, 
				_ => WaveType.None, 
			};
		}
	}

	public override void Reset()
	{
		type = WaveType.None;
		base.Reset();
	}
}

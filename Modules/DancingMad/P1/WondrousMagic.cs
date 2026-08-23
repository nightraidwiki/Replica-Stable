using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P1;

public class WondrousMagic : ISpecialAction
{
	public bool? lockon;

	public List<IGameObject> shareLockon = new List<IGameObject>();

	public override string Name => "Wondrous Magic";

	public override HashSet<uint> ActionID => new HashSet<uint> { 47768u, 47774u, 47775u, 47777u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 47768 || actionId == 47774)
		{
			SimpleElement.Fan(info.Pos, 40f, 90, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 47768u, 47774u }
			});
		}
		actionId = info.ActionId;
		if (actionId == 47775 || actionId == 47777)
		{
			SimpleElement.Rectangle(info.Pos, 40f, 5f, 0f, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 47775u, 47777u }
			});
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (Source.BaseId == 19504 && icon - 673 <= 1)
		{
			lockon = icon == 674;
		}
		switch (icon)
		{
		case 127u:
			if (!lockon.HasValue)
			{
				break;
			}
			if (lockon == true)
			{
				foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
				{
					SimpleLockon.TarLockOn5m5s(allPlayer, 1000f);
				}
			}
			else
			{
				SimpleLockon.Share5S(PlayerHelper.Healer.FirstOrDefault(), 1000f);
				SimpleLockon.Share5S(PlayerHelper.DPS.FirstOrDefault(), 1000f);
			}
			lockon = null;
			break;
		case 128u:
			shareLockon.Add(Source);
			if (!lockon.HasValue || shareLockon.Count != 2)
			{
				break;
			}
			if (lockon == true)
			{
				foreach (IGameObject item in shareLockon)
				{
					SimpleLockon.Share5S(item, 1000f);
				}
			}
			else
			{
				foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
				{
					SimpleLockon.TarLockOn5m5s(allPlayer2, 1000f);
				}
			}
			lockon = null;
			shareLockon.Clear();
			break;
		}
	}

	public override void Reset()
	{
		lockon = null;
		shareLockon.Clear();
		base.Reset();
	}
}

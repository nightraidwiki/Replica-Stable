using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M6S;

public class ColorClash : ISpecialAction
{
	private bool? partnerStack;

	public override string Name => "Color Clash";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42635u, 42637u };

	public override void OnActionCast(ActorCastInfo info)
	{
		partnerStack = info.ActionId == 42637;
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 4163)
		{
			return;
		}
		bool? flag = partnerStack;
		if (!flag.HasValue)
		{
			return;
		}
		if (flag == true)
		{
			foreach (IGameObject dP in PlayerHelper.DPS)
			{
				SimpleLockon.ShareLockon2(dP);
			}
		}
		else
		{
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				SimpleLockon.ShareLockon(item);
			}
		}
		partnerStack = null;
	}

	public override void Reset()
	{
		base.Reset();
		partnerStack = null;
	}
}

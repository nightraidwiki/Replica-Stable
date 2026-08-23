using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P3;

public class ChaosFlame : ISpecialAction
{
	public override string Name => "Chaos Flame";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47859u };

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 1600)
		{
			IGameObject gameObject = info.TargetID.GameObject();
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 5f,
					radiusZ = 5f,
					destroyTime = 50000f,
					delayDrawTime = (info.Time - 5f) * 1000f,
					StatusCheck = new StatusCheck
					{
						CheckObject = gameObject,
						Status = 1600u
					}
				}, gameObject);
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (base.NumCasts > 0)
		{
			return;
		}
		base.NumCasts++;
		IGameObject checkObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 2015290);
		DrawElement element = new DrawElement
		{
			drawAvfx = "gl_donut1807_o0g",
			radiusX = 10f,
			radiusZ = 10f,
			distanceCheck = new DistanceCheck
			{
				CheckObject = checkObject,
				CheckType = 2,
				Count = 2
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 47860u }
			}
		};
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(element, allPlayer);
		}
	}
}

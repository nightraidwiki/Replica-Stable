using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zelenia;

public class EscelonsFall : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Near,
		Far
	}

	private Mechanic curMechanic;

	private Queue<Mechanic> mechanicList = new Queue<Mechanic>();

	public override string Name => "EscelonsFall";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 43181u, 43182u, 43183u };

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 2970)
		{
			return;
		}
		if (curMechanic == Mechanic.None)
		{
			curMechanic = ((info.Stack == 758) ? Mechanic.Near : Mechanic.Far);
			new TimeHelper(6000L, delegate
			{
				drawMechanic();
			});
		}
		else
		{
			mechanicList.Enqueue((info.Stack == 758) ? Mechanic.Near : Mechanic.Far);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 43183)
		{
			return;
		}
		base.NumCasts++;
		if (base.NumCasts % 4 == 0)
		{
			new TimeHelper(500L, delegate
			{
				drawMechanic();
			});
		}
	}

	public void drawMechanic()
	{
		if (curMechanic == Mechanic.None)
		{
			return;
		}
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 18374);
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				radiusX = 48f,
				radiusZ = 48f,
				drawOnObject = true,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckObject = allPlayer,
					CheckType = ((curMechanic != Mechanic.Near) ? 1 : 0),
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 43183u }
				}
			}, target);
		}
		if (mechanicList.Count != 0)
		{
			curMechanic = mechanicList.Dequeue();
			return;
		}
		curMechanic = Mechanic.None;
		Reset();
	}

	public override void Reset()
	{
		curMechanic = Mechanic.None;
		mechanicList.Clear();
		base.NumCasts = 0;
	}
}

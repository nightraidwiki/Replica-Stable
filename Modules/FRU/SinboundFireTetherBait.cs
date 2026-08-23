using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class SinboundFireTetherBait : ISpecialAction
{
	private enum Mechanic
	{
		Fire,
		Thunder
	}

	private List<Mechanic> mechanicQueue = new List<Mechanic>();

	private List<IGameObject?> gameObjectQueue = new List<IGameObject>();

	public override string Name => "Sinbound Fire (tether + bait)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40170u, 40171u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 40170)
		{
			mechanicQueue.Clear();
			gameObjectQueue.Clear();
			base.CanDraw = true;
		}
		if (info.ActionId != 40171 || !base.CanDraw)
		{
			return;
		}
		int num = gameObjectQueue.IndexOf(info.Target);
		if (num == -1)
		{
			return;
		}
		switch (mechanicQueue[num])
		{
		case Mechanic.Fire:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan090_1bf",
					radiusX = 60f,
					radiusZ = 60f,
					target = allPlayer,
					destroyTime = 3400f,
					distanceCheck = new DistanceCheck
					{
						CheckObject = info.Target,
						CheckType = 0
					}
				}, info.Target);
			}
			break;
		}
		case Mechanic.Thunder:
		{
			foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan120_1bpxf",
					radiusX = 60f,
					radiusZ = 60f,
					target = allPlayer2,
					destroyTime = 3400f,
					distanceCheck = new DistanceCheck
					{
						CheckObject = info.Target,
						CheckType = 0,
						Count = 3
					}
				}, info.Target);
			}
			break;
		}
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (!base.CanDraw)
		{
			return;
		}
		if (Id == 287)
		{
			mechanicQueue.Add(Mechanic.Thunder);
			gameObjectQueue.Add(targetId.GameObject());
			SimpleElement.ShowText("Thunder", RaptureAtkModule.TextGimmickHintStyle.Info, 1);
		}
		if (Id == 249)
		{
			mechanicQueue.Add(Mechanic.Fire);
			gameObjectQueue.Add(targetId.GameObject());
			SimpleElement.ShowText("Fire", RaptureAtkModule.TextGimmickHintStyle.Warning, 1);
		}
		if (mechanicQueue.Count == 4)
		{
			IEnumerable<string> elements = from mechanic in mechanicQueue.Take(4)
				select (mechanic == Mechanic.Fire) ? "Fire" : "Thunder";
			new TimeHelper(3000L, delegate
			{
				SimpleElement.ShowText(string.Join(" -> ", elements) ?? "", RaptureAtkModule.TextGimmickHintStyle.Warning, 12);
			});
		}
	}

	public override void Reset()
	{
		mechanicQueue.Clear();
		gameObjectQueue.Clear();
		base.Reset();
	}
}

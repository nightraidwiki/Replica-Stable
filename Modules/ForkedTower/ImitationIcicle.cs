using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.ForkedTower;

public class ImitationIcicle : ISpecialAction
{
	private readonly List<Vector3> circlePos = new List<Vector3>();

	private readonly List<Vector3> crossPos = new List<Vector3>();

	public override string Name => "Imitation Icicle";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30343u, 42773u, 30210u, 30228u };

	public override uint Phase => 3u;

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			int count = aoes.Count;
			if (count == 0)
			{
				return Array.Empty<StaticVfx>();
			}
			long drawTime = aoes[0].DrawTime;
			int i;
			for (i = 0; i < count && aoes[i].DrawTime - drawTime < 1500; i++)
			{
			}
			return aoes.Slice(0, i);
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 2014546)
		{
			circlePos.Add(GameObject.Position);
		}
		else if (GameObject.BaseId == 2014547)
		{
			crossPos.Add(GameObject.Position);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 42773)
		{
			return;
		}
		foreach (Vector3 circlePo in circlePos)
		{
			if (info.Pos.AlmostEqual(circlePo, 1f))
			{
				DrawElement element = new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = info.Pos,
					drawOnObject = false,
					radiusX = 20f,
					radiusZ = 20f,
					destroyTime = Environment.TickCount64,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 30210u },
						TargetHitCount = 6
					}
				};
				aoes.Add(DrawManager.Draw(element));
			}
		}
		foreach (Vector3 crossPo in crossPos)
		{
			if (info.Pos.AlmostEqual(crossPo, 1f))
			{
				DrawElement drawElement = new DrawElement
				{
					drawAvfx = "general_x02f",
					Position = info.Pos,
					drawOnObject = false,
					radiusX = 8f,
					radiusZ = 60f,
					refRotation = info.Facing,
					destroyTime = Environment.TickCount64,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 30228u },
						TargetHitCount = 2
					}
				};
				aoes.Add(DrawManager.Draw(drawElement));
				drawElement.refRotation += 90.Degrees();
				aoes.Add(DrawManager.Draw(drawElement));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 30343)
		{
			circlePos.Clear();
			crossPos.Clear();
		}
		uint actionId = info.ActionId;
		if (actionId == 30210 || actionId == 30228)
		{
			List<StaticVfx> list = aoes.Where((StaticVfx x) => x.Position.AlmostEqual(info.Source.Position, 1f)).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				StaticVfx staticVfx = list[num];
				staticVfx.Remove();
				aoes.Remove(staticVfx);
			}
		}
	}

	public override void Reset()
	{
		circlePos.Clear();
		crossPos.Clear();
		base.Reset();
	}
}

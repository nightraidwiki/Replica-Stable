using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.FRU;

public class WingsOfLightDarkTankCleave : ISpecialAction
{
	private int tower;

	private readonly (int index, Vector3 pos)[] map = new(int, Vector3)[3]
	{
		(51, new Vector3(94f, 0f, 96.5f)),
		(52, new Vector3(106f, 0f, 96.5f)),
		(53, new Vector3(100f, 0f, 107f))
	};

	private readonly List<int> history = new List<int>();

	public static IGameObject? MT;

	public override string Name => "Wings of Light/Dark (tank cleave)";

	public override uint Phase => 5u;

	public override uint WeatherID => 108u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40319u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (MT == Svc.Objects.LocalPlayer)
			{
				return aoes.Take(1);
			}
			if (PlayerHelper.Tank.FirstOrDefault((IGameObject o) => o != MT) == Svc.Objects.LocalPlayer)
			{
				return aoes.TakeLast(1);
			}
			return Array.Empty<StaticVfx>();
		}
	}

	private Vector3 relSouth
	{
		get
		{
			if (history.Count <= 0)
			{
				return default(Vector3);
			}
			return new Vector3(100f, 0f, 100f) - map[history[0] - 51].pos;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		MT = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839).TargetObject;
		tower = 0;
		history.Clear();
		aoes.Clear();
	}

	public override void OnEnvControl(byte index, uint state)
	{
		if ((index != 51 && index != 52 && index != 53) || state != 131073 || history.Contains(index))
		{
			return;
		}
		history.Add(index);
		tower++;
		if (tower == 1)
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839);
			Vector3 item = map[index - 51].pos;
			DrawElement element = new DrawElement
			{
				Enable = false,
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = item,
				drawOnObject = false,
				radiusX = 3f,
				radiusZ = 60f,
				target = target,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40314u, 40315u },
					TargetHitCount = tower
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
		else if (tower == 2)
		{
			Vector3 targetPosition = new Vector3(100f, 0f, 100f) + STSafePos();
			DrawElement element2 = new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = new Vector3(100f, 0f, 100f),
				drawOnObject = false,
				radiusX = 3f,
				radiusZ = 60f,
				targetPosition = targetPosition,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40314u, 40315u },
					TargetHitCount = tower
				}
			};
			aoes.Add(DrawManager.Draw(element2));
		}
	}

	private Vector3 STSafePos()
	{
		Angle angle = MathF.Atan2(relSouth.X, relSouth.Z).Radians();
		return (60f * (angle - (WingsOfLightDarkCleave.LightFirst ? 120.Degrees() : (-120.Degrees()))).ToDirection()).ToVec3();
	}

	public override void Reset()
	{
		tower = 0;
		history.Clear();
		MT = null;
		base.Reset();
	}
}

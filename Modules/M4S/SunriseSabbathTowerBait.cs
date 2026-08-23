using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class SunriseSabbathTowerBait : ISpecialAction
{
	private readonly List<IGameObject> birds = new List<IGameObject>();

	public override string Name => "Sunrise Sabbath (tower bait)";

	public override uint Phase => 8u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38417u, 38419u };

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 17323 && id == 4565)
		{
			birds.Add(source);
			if (birds.Count == 2)
			{
				AddTower();
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (birds.Count == 2)
		{
			AddTower();
		}
		birds.Clear();
	}

	private void AddTower()
	{
		if ((!Svc.Objects.LocalPlayer.HasStatus(4000u) && !Svc.Objects.LocalPlayer.HasStatus(4001u)) || ((Svc.Objects.LocalPlayer.GetStatusRemainingTime(4000u, out var time) || Svc.Objects.LocalPlayer.GetStatusRemainingTime(4001u, out time)) && time > 15f))
		{
			return;
		}
		bool flag = false;
		foreach (IGameObject bird in birds)
		{
			bool flag2 = ((int)Math.Round(bird.Rotation * 180f / (float)Math.PI / 45f) & 1) != 0;
			flag = Math.Abs(165f - (new WPos(bird.Position) + bird.Rotation.Radians().ToDirection() * (flag2 ? 21.213203f : 30f)).Z) <= 1f;
		}
		DrawElement element = new DrawElement
		{
			drawAvfx = "co_trap00h1",
			Position = (flag ? new Vector3(107.6f, 0f, 152.4f) : new Vector3(112.5f, 0f, 157.5f)),
			drawOnObject = false,
			radiusX = 1f,
			radiusY = 5f,
			radiusZ = 1f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
			}
		};
		DrawElement element2 = new DrawElement
		{
			drawAvfx = "co_trap00h1",
			Position = (flag ? new Vector3(107.6f, 0f, 177.6f) : new Vector3(112.5f, 0f, 172.5f)),
			drawOnObject = false,
			radiusX = 1f,
			radiusY = 5f,
			radiusZ = 1f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
			}
		};
		DrawElement element3 = new DrawElement
		{
			drawAvfx = "co_trap00h1",
			Position = (flag ? new Vector3(92.4f, 0f, 177.6f) : new Vector3(87.5f, 0f, 172.5f)),
			drawOnObject = false,
			radiusX = 1f,
			radiusY = 5f,
			radiusZ = 1f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
			}
		};
		DrawElement element4 = new DrawElement
		{
			drawAvfx = "co_trap00h1",
			Position = (flag ? new Vector3(92.4f, 0f, 152.4f) : new Vector3(87.5f, 0f, 157.5f)),
			drawOnObject = false,
			radiusX = 1f,
			radiusY = 5f,
			radiusZ = 1f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38437u, 38438u, 39257u, 39258u }
			}
		};
		DrawManager.Draw(element, Svc.Objects.LocalPlayer);
		DrawManager.Draw(element2, Svc.Objects.LocalPlayer);
		DrawManager.Draw(element3, Svc.Objects.LocalPlayer);
		DrawManager.Draw(element4, Svc.Objects.LocalPlayer);
	}

	public override void Reset()
	{
		birds.Clear();
	}
}

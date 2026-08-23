using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class SunriseSabbathSoaringSoulpress : ISpecialAction
{
	private readonly List<IGameObject> birds = new List<IGameObject>();

	public override string Name => "Sunrise Sabbath (tower)";

	public override uint Phase => 8u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38417u, 38419u };

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 17323 && id == 4565)
		{
			birds.Add(source);
			AddTower(source);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		foreach (IGameObject bird in birds)
		{
			AddTower(bird);
		}
		birds.Clear();
	}

	private static void AddTower(IGameObject bird)
	{
		if ((!Svc.Objects.LocalPlayer.GetStatusRemainingTime(4000u, out var time) && !Svc.Objects.LocalPlayer.GetStatusRemainingTime(4001u, out time)) || !(time < 15f))
		{
			bool flag = ((int)Math.Round(bird.Rotation * 180f / (float)Math.PI / 45f) & 1) != 0;
			WPos wPos = new WPos(bird.Position) + bird.Rotation.Radians().ToDirection() * (flag ? 21.213203f : 30f);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "share_trap01k1",
				Position = wPos.ToVec3(),
				drawOnObject = false,
				radiusX = 3f,
				radiusY = 5f,
				radiusZ = 3f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38440u, 39259u, 39297u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}

	public override void Reset()
	{
		birds.Clear();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M9S;

public class BatStomp : ISpecialAction
{
	public override string Name => "Bat Stomp";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45940u, 45941u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (aoes.Count == 0)
			{
				return Array.Empty<StaticVfx>();
			}
			int numCasts = base.NumCasts;
			IEnumerable<StaticVfx> source = aoes;
			return source.Take(numCasts switch
			{
				2 => 3, 
				5 => 5, 
				_ => 2, 
			});
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 1957)
		{
			int targetHitCount = info.Stack switch
			{
				37u => 2, 
				51u => 5, 
				55u => 10, 
				_ => 0, 
			};
			IGameObject gameObject = info.TargetID.GameObject();
			Angle dir = ((gameObject.Rotation.Radians().ToDirection().OrthoL()
				.Dot(gameObject.DirectionTo(new WPos(100f, 100f))) > 0f) ? 1 : (-1)) * 90.Degrees();
			WDir wDir = new WPos(gameObject.Position) - new WPos(100f, 100f);
			WPos wPos = new WPos(100f, 100f) + wDir.Rotate(dir);
			DrawElement element = new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = wPos.ToVec3(),
				drawOnObject = false,
				radiusX = 8f,
				radiusZ = 8f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45941u },
					TargetHitCount = targetHitCount
				}
			};
			aoes.Add(DrawManager.Draw(element));
			aoes.SortBy((StaticVfx v) => v.HitCounter.TargetHitCount);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 45940)
		{
			Reset();
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 45941)
		{
			if (aoes.Count > 0)
			{
				aoes[0].Remove();
				aoes.RemoveAt(0);
			}
			base.NumCasts++;
		}
	}
}

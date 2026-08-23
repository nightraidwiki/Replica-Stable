using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.LockWyvernEx;

public class Charge : ISpecialAction
{
	public override string Name => "Charge";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43908u, 43909u, 43910u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 43908)
		{
			IGameObject gameObject = info.SourceId.GameObject();
			Vector3 vector = info.TargetPos - gameObject.Position;
			Angle refRotation = MathF.Atan2(vector.X, vector.Z).Radians();
			DrawElement element = new DrawElement
			{
				drawAvfx = "general02xf",
				Position = gameObject.Position,
				drawOnObject = false,
				radiusX = 6f,
				refRotation = refRotation,
				fixRotation = true,
				targetPosition = info.TargetPos,
				endToTarget = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 43909u, 43910u },
					TargetHitCount = 3
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 43909 <= 1 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

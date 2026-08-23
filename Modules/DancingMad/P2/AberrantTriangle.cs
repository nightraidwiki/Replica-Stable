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

namespace Replica.Modules.DancingMad.P2;

public class AberrantTriangle : ISpecialAction
{
	public override string Name => "Aberrant Triangle";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47840u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			if (aoes.Count == 0)
			{
				return Array.Empty<StaticVfx>();
			}
			long initTime = aoes[0].initTime;
			int i;
			for (i = 0; i < aoes.Count && aoes[i].initTime - initTime < 1000; i++)
			{
			}
			return aoes.Take(i);
		}
	}

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		IGameObject gameObject = actorID.GameObject();
		if (gameObject == null || gameObject.BaseId - 2015154 > 1 || p1 != 16 || p2 != 32)
		{
			return;
		}
		Vector3 vector;
		if (gameObject.BaseId == 2015154)
		{
			vector = new Vector3(gameObject.Position.X - 3f, gameObject.Position.Y, gameObject.Position.Z + 5f);
			if (gameObject.Position.AlmostEqual(new Vector3(88.453f, 0f, 90f), 1f))
			{
				vector = new Vector3(gameObject.Position.X + 3f, gameObject.Position.Y, gameObject.Position.Z + 5f);
			}
		}
		else
		{
			vector = new Vector3(gameObject.Position.X + 3f, gameObject.Position.Y, gameObject.Position.Z + 5f);
		}
		for (int i = 0; i < 3; i++)
		{
			Vector2 vector2 = (vector - gameObject.Position).ToVector2().RotationDegress(i * 120);
			Vector3 position = gameObject.Position + new Vector3(vector2.X, 0f, vector2.Y);
			DrawElement element = new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				Position = position,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47840u },
					TargetHitCount = 21
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}

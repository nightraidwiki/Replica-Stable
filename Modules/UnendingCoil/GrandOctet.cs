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

namespace Replica.Modules.UnendingCoil;

public class GrandOctet : ISpecialAction
{
	public override string Name => "Grand Octet";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9959u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (base.CanDraw && source.BaseId == 8168 && id == 7747)
		{
			base.CanDraw = false;
			bool flag = Math.Abs(source.Position.X) < 1f || Math.Abs(source.Position.Z) < 1f;
			Vector3 vector = source.Position - new Vector3(0f, 0f, 0f);
			Angle angle = (MathF.Atan2(vector.X, vector.Z) + (float)Math.PI).Radians();
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = new Vector3(0f, 0f, 0f),
				drawOnObject = false,
				radiusX = 2f,
				radiusZ = 21f,
				refRotation = angle,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9923u }
				}
			}, Svc.Objects.LocalPlayer);
			WDir wDir = 20f * angle.ToDirection();
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = wDir.ToVec3(),
				drawOnObject = false,
				radiusX = 1.5f,
				radiusZ = 7f,
				refRotation = angle + (flag ? 90.Degrees() : (-90.Degrees())),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9934u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}

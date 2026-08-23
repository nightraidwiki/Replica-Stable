using System;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.JeunoArc1;

public class Rage : ISpecialAction
{
	public override string Name => "Rage";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41073u, 41074u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 41073)
		{
			base.NumCasts++;
			Vector3 position = info.SourceId.GameObject().Position;
			Vector3 vector = info.Pos - position;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = position,
				drawOnObject = false,
				radiusX = 5f,
				targetPosition = info.Pos,
				endToTarget = true,
				refRotation = new Angle(MathF.Atan2(vector.X, vector.Z)),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41075u },
					TargetHitCount = base.NumCasts
				}
			}, Svc.Objects.LocalPlayer);
		}
		else
		{
			SimpleElement.Circle(info.Pos, 20f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 41077u }
			});
			base.NumCasts = 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class HeavenlyStrike : ISpecialAction
{
	private readonly List<Vector3> pos = new List<Vector3>();

	public override string Name => "Heavenly Strike (knockback)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40198u, 40208u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 40198 && pos.Count < 2)
		{
			pos.Add(info.Pos);
		}
		if (info.ActionId != 40208)
		{
			return;
		}
		foreach (Vector3 po in pos)
		{
			Vector3 vector = po - new Vector3(100f, 0f, 100f);
			Angle refRotation = new Angle(MathF.Atan2(vector.X, vector.Z));
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = new Vector3(100f, 0f, 100f),
				drawOnObject = false,
				radiusX = 2f,
				radiusZ = 18f,
				refRotation = refRotation,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40207u }
				}
			});
		}
		pos.Clear();
	}

	public override void Reset()
	{
		pos.Clear();
		base.Reset();
	}
}

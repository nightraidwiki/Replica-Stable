using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Everkeep;

public class ChasmOfVollok : ISpecialAction
{
	private static readonly List<Vector2> Offset;

	public override string Name => "Chasm of Vollok";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37785u, 37779u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37785)
		{
			foreach (Vector2 item in Offset)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "d1070_laser_01c2",
					radiusX = 2.5f,
					radiusZ = 5f,
					refOffsetX = item.X,
					refOffsetZ = 2.5f + item.Y,
					refRotation = info.Facing,
					fixRotation = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { info.ActionId }
					}
				}, info.SourceId.GameObject());
			}
		}
		if (info.ActionId != 37779)
		{
			return;
		}
		foreach (Vector2 item2 in Offset)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "d1070_laser_01c2",
				radiusX = 5f,
				radiusZ = 10f,
				refOffsetX = item2.X,
				refOffsetZ = 5f + item2.Y,
				refRotation = info.Facing,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
		}
	}

	static ChasmOfVollok()
	{
		int num = 4;
		List<Vector2> list = new List<Vector2>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Vector2> span = CollectionsMarshal.AsSpan(list);
		span[0] = new Vector2(30f, 0f);
		span[1] = new Vector2(-30f, 0f);
		span[2] = new Vector2(0f, 30f);
		span[3] = new Vector2(0f, -30f);
		Offset = list;
	}
}

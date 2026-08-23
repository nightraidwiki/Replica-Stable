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

namespace Replica.Modules.M1S;

public class ElevateAndEviscerate : ISpecialAction
{
	private IGameObject? iconTarget;

	public override string Name => "Elevate and Eviscerate";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37958u, 37960u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (iconTarget == Svc.Objects.LocalPlayer)
		{
			switch (info.ActionId)
			{
			case 37958:
				SimpleElement.ShowText("Knockup");
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 10f,
					drawOnObject = true,
					refColor = new Vector4(1f, 1f, 1f, 3f),
					refTargetColor = new Vector4(1f, 1f, 1f, 3f),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 37962u }
					}
				}, iconTarget);
				break;
			case 37960:
				SimpleElement.ShowText("Slam down");
				break;
			}
		}
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 538)
		{
			return;
		}
		iconTarget = target;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 5f,
			radiusZ = 60f,
			drawOnObject = false,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 37962u }
			},
			PositionCustomAction = delegate
			{
				Vector3 result = new Vector3(0f, 0f, 0f);
				(int, int, int)[] array = new(int, int, int)[4]
				{
					(int.MinValue, 90, 85),
					(90, 100, 95),
					(100, 110, 105),
					(110, int.MaxValue, 115)
				};
				(int, int, int)[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					(int, int, int) tuple = array2[i];
					if (target.Position.X >= (float)tuple.Item1 && target.Position.X < (float)tuple.Item2)
					{
						result.X = tuple.Item3;
						break;
					}
				}
				array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					(int, int, int) tuple2 = array2[j];
					if (target.Position.Z >= (float)tuple2.Item1 && target.Position.Z < (float)tuple2.Item2)
					{
						result.Z = tuple2.Item3;
						break;
					}
				}
				return result;
			}
		}, target);
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 5f,
			radiusZ = 60f,
			drawOnObject = false,
			refRotation = 90.Degrees(),
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 37962u }
			},
			PositionCustomAction = delegate
			{
				Vector3 result = new Vector3(0f, 0f, 0f);
				(int, int, int)[] array = new(int, int, int)[4]
				{
					(int.MinValue, 90, 85),
					(90, 100, 95),
					(100, 110, 105),
					(110, int.MaxValue, 115)
				};
				(int, int, int)[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					(int, int, int) tuple = array2[i];
					if (target.Position.X >= (float)tuple.Item1 && target.Position.X < (float)tuple.Item2)
					{
						result.X = tuple.Item3;
						break;
					}
				}
				array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					(int, int, int) tuple2 = array2[j];
					if (target.Position.Z >= (float)tuple2.Item1 && target.Position.Z < (float)tuple2.Item2)
					{
						result.Z = tuple2.Item3;
						break;
					}
				}
				return result;
			}
		}, target);
	}
}

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

namespace Replica.Modules.TEA;

public class HawkBlasterP1 : ISpecialAction
{
	private float blasterStartingDirection;

	private const float BlasterOffset = 14f;

	public override string Name => "Hawk Blaster";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18480u };

	private int NextBlasterIndex
	{
		get
		{
			switch (base.NumCasts)
			{
			case 0:
			case 1:
				return 0;
			case 2:
			case 3:
				return 1;
			case 4:
			case 5:
				return 2;
			case 6:
			case 7:
				return 3;
			case 8:
				return 4;
			case 9:
			case 10:
				return 5;
			case 11:
			case 12:
				return 6;
			case 13:
			case 14:
				return 7;
			case 15:
			case 16:
				return 8;
			case 17:
				return 9;
			default:
				return 10;
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (base.NumCasts == 0)
		{
			(float, float) tuple = (info.Pos.X - 100f, info.Pos.Z - 100f);
			if ((Math.Abs(tuple.Item2) < 2f) ? (tuple.Item1 > 0f) : (tuple.Item2 > 0f))
			{
				tuple = (0f - tuple.Item1, 0f - tuple.Item2);
			}
			blasterStartingDirection = MathF.Atan2(tuple.Item1, tuple.Item2);
			base.NumCasts = 2;
		}
		BlasterCenters(NextBlasterIndex);
		base.NumCasts++;
	}

	private void BlasterCenters(int index)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 10f,
			radiusZ = 10f,
			drawOnObject = false,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18480u },
				TargetHitCount = 2
			}
		};
		switch (index)
		{
		case 0:
		case 1:
		case 2:
		case 3:
		{
			float num3 = MathF.Sin(blasterStartingDirection - (float)index * 45.Degrees().Rad);
			float num4 = MathF.Cos(blasterStartingDirection - (float)index * 45.Degrees().Rad);
			drawElement.Position = new Vector3(100f, 0f, 100f) + new Vector3(num3 * 14f, 0f, num4 * 14f);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			drawElement.Position = new Vector3(100f, 0f, 100f) - new Vector3(num3 * 14f, 0f, num4 * 14f);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		}
		case 5:
		case 6:
		case 7:
		case 8:
		{
			float num = MathF.Sin(blasterStartingDirection - (float)(index - 5) * 45.Degrees().Rad);
			float num2 = MathF.Cos(blasterStartingDirection - (float)(index - 5) * 45.Degrees().Rad);
			drawElement.Position = new Vector3(100f, 0f, 100f) + new Vector3(num * 14f, 0f, num2 * 14f);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			drawElement.Position = new Vector3(100f, 0f, 100f) - new Vector3(num * 14f, 0f, num2 * 14f);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		}
		case 4:
		case 9:
			drawElement.Position = new Vector3(100f, 0f, 100f);
			DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
			break;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.E3;

public class NightRaidCommand24Portals : ISpecialAction
{
	public int DoorIndex;

	public int FirstStatus;

	public HashSet<int> DoorIndexs = new HashSet<int>();

	public Dictionary<int, (string first, string second)> DoorMap = new Dictionary<int, (string, string)>
	{
		{
			11,
			("Red", "Blue")
		},
		{
			12,
			("Blue", "Red")
		}
	};

	public Dictionary<string, int[]> refZPatterns = new Dictionary<string, int[]>
	{
		{
			"Red_12",
			new int[2] { 95, 115 }
		},
		{
			"Red_34",
			new int[2] { 85, 105 }
		},
		{
			"Red_67",
			new int[2] { 85, 105 }
		},
		{
			"Red_89",
			new int[2] { 95, 115 }
		},
		{
			"Blue_12",
			new int[2] { 85, 105 }
		},
		{
			"Blue_34",
			new int[2] { 95, 115 }
		},
		{
			"Blue_67",
			new int[2] { 95, 115 }
		},
		{
			"Blue_89",
			new int[2] { 85, 105 }
		}
	};

	public override string Name => "Night Raid Command (2+4 portals)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19516u, 19517u, 19518u, 19521u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnEnvControl(byte index, uint state)
	{
		if (state == 131073)
		{
			if ((uint)(index - 11) <= 1u)
			{
				DoorIndex = index;
			}
			if ((uint)(index - 1) <= 3u || (uint)(index - 6) <= 3u)
			{
				DoorIndexs.Add(index);
			}
		}
		if (state == 524292)
		{
			DoorIndex = 0;
			DoorIndexs.Clear();
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (Svc.Objects.LocalPlayer.HasStatus(2238u))
		{
			FirstStatus = 2238;
		}
		if (Svc.Objects.LocalPlayer.HasStatus(2239u))
		{
			FirstStatus = 2239;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		bool flag = (actionId == 19516 || actionId == 19518) && FirstStatus == 2238;
		if (!flag)
		{
			uint actionId2 = info.ActionId;
			flag = (actionId2 == 19517 || actionId2 == 19521) && FirstStatus == 2239;
		}
		if (!flag || !DoorMap.TryGetValue(DoorIndex, out (string, string) value) || info.Source.Position.X < 90f || info.Source.Position.X > 110f)
		{
			return;
		}
		DrawElement drawElement = new DrawElement
		{
			Enable = false,
			drawAvfx = "general02xf",
			drawOnObject = false,
			radiusX = 5f,
			radiusZ = 100f,
			refRotation = -90.Degrees(),
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 19520u }
			}
		};
		string text;
		if (info.Source.Position.X < 100f)
		{
			(text, _) = value;
		}
		else
		{
			text = value.Item2;
		}
		string value2 = text;
		string key = $"{value2}_{DoorIndexs.First()}{DoorIndexs.Last()}";
		if (refZPatterns.TryGetValue(key, out int[] value3))
		{
			int[] array = value3;
			foreach (int num in array)
			{
				drawElement.Position = new Vector3(120f, 0f, num);
				aoes.Add(DrawManager.Draw(drawElement));
			}
		}
		DoorIndex = 0;
		DoorIndexs.Clear();
	}

	public override void Reset()
	{
		FirstStatus = 0;
		DoorIndex = 0;
		DoorIndexs.Clear();
		base.Reset();
	}
}

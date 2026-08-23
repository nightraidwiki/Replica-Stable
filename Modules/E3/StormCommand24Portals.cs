using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.E3;

public class StormCommand24Portals : ISpecialAction
{
	public int DoorIndex;

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
			new int[4] { 85, 105, 95, 115 }
		},
		{
			"Red_34",
			new int[4] { 95, 115, 85, 105 }
		},
		{
			"Red_67",
			new int[4] { 95, 115, 85, 105 }
		},
		{
			"Red_89",
			new int[4] { 85, 105, 95, 115 }
		},
		{
			"Blue_12",
			new int[4] { 95, 115, 85, 105 }
		},
		{
			"Blue_34",
			new int[4] { 85, 105, 95, 115 }
		},
		{
			"Blue_67",
			new int[4] { 85, 105, 95, 115 }
		},
		{
			"Blue_89",
			new int[4] { 95, 115, 85, 105 }
		}
	};

	public override string Name => "Storm Command (2+4 portals)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19518u, 19520u };

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

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (Svc.Objects.LocalPlayer.StatusList.Any((IStatus status) => status.StatusId - 2238 <= 1))
		{
			return;
		}
		if (info.ActionId == 19518 && DoorMap.TryGetValue(DoorIndex, out (string, string) value))
		{
			DrawElement drawElement = new DrawElement
			{
				Enable = false,
				drawAvfx = "general02xf",
				drawOnObject = false,
				radiusX = 5f,
				radiusZ = 55f,
				refRotation = (DoorIndexs.Any((int x) => x <= 4) ? 90.Degrees() : (-90.Degrees())),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 19520u },
					TargetHitCount = 4
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
				foreach (int num2 in array)
				{
					drawElement.Position = new Vector3(DoorIndexs.Any((int x) => x <= 4) ? 80 : 120, 0f, num2);
					aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				}
			}
			DoorIndex = 0;
			DoorIndexs.Clear();
		}
		if (info.ActionId == 19520 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}

	public override void Reset()
	{
		DoorIndex = 0;
		DoorIndexs.Clear();
		base.Reset();
	}
}

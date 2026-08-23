using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P12S.P12S;

public class ClassicalConcepts : ISpecialAction
{
	private int cubeCount;

	private int[,] cube = new int[4, 3];

	private IGameObject[,] cubeGameObject = new IGameObject[4, 3];

	private List<(int, int)> bias;

	public override string Name => "Classical Concepts";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	private void DrawLines(bool swap)
	{
		if (swap)
		{
			for (int i = 0; i < 4; i++)
			{
				ref int reference = ref cube[i, 0];
				ref int reference2 = ref cube[3 - i, 2];
				int num = cube[3 - i, 2];
				int num2 = cube[i, 0];
				reference = num;
				reference2 = num2;
				if (i < 2)
				{
					reference = ref cube[i, 1];
					ref int reference3 = ref cube[3 - i, 1];
					num2 = cube[3 - i, 1];
					num = cube[i, 1];
					reference = num2;
					reference3 = num;
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			Plugin.DebugLog($"{cube[0, j]}, {cube[1, j]}, {cube[2, j]}, {cube[3, j]}");
		}
		for (int k = 0; k < 4; k++)
		{
			for (int l = 0; l < 3; l++)
			{
				if (cube[k, l] != 2)
				{
					continue;
				}
				List<(int, int)> list = new List<(int, int)>();
				List<(int, int)> list2 = new List<(int, int)>();
				foreach (var item in bias)
				{
					int num3 = k + item.Item1;
					int num4 = l + item.Item2;
					if (num3 >= 0 && num3 <= 3 && num4 >= 0 && num4 <= 2)
					{
						if (cube[num3, num4] == 1)
						{
							list.Add((num3, num4));
						}
						if (cube[num3, num4] == 3)
						{
							list2.Add((num3, num4));
						}
					}
				}
				if (list.Count == 1)
				{
					DrawLine(k, l, list[0].Item1, list[0].Item2, isRed: true, swap);
				}
				else
				{
					int num5 = 0;
					foreach (var item2 in bias)
					{
						int num6 = list[0].Item1 + item2.Item1;
						int num7 = list[0].Item2 + item2.Item2;
						if (num6 >= 0 && num6 <= 3 && num7 >= 0 && num7 <= 2 && cube[num6, num7] == 2)
						{
							num5++;
						}
					}
					int index = ((num5 == 2) ? 1 : 0);
					DrawLine(k, l, list[index].Item1, list[index].Item2, isRed: true, swap);
				}
				if (list2.Count == 1)
				{
					DrawLine(k, l, list2[0].Item1, list2[0].Item2, isRed: false, swap);
					continue;
				}
				int num8 = 0;
				foreach (var item3 in bias)
				{
					int num9 = list2[0].Item1 + item3.Item1;
					int num10 = list2[0].Item2 + item3.Item2;
					if (num9 >= 0 && num9 <= 3 && num10 >= 0 && num10 <= 2 && cube[num9, num10] == 2)
					{
						num8++;
					}
				}
				int index2 = ((num8 == 2) ? 1 : 0);
				DrawLine(k, l, list2[index2].Item1, list2[index2].Item2, isRed: false, swap);
			}
		}
	}

	private void DrawLine(int x1, int y1, int x2, int y2, bool isRed, bool swap)
	{
		Plugin.DebugLog($"draw: ({x1}, {y1}) => ({x2}, {y2})");
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = (isRed ? "general02xf" : "general02pxf"),
			radiusX = 1f,
			radiusY = 1f,
			target = cubeGameObject[x2, y2],
			drawOnObject = true,
			endToTarget = true,
			delayDrawTime = (swap ? 5000 : 0),
			hitCounter = new HitCounter
			{
				ActionID = (swap ? new HashSet<uint> { 33591u } : new HashSet<uint> { 33587u })
			}
		}, cubeGameObject[x1, y1]);
	}

	public override void OnObjectCreatedEvent(IGameObject gameObject)
	{
		if ((gameObject != null && gameObject.BaseId == 16183) || (gameObject != null && gameObject.BaseId == 16184) || (gameObject != null && gameObject.BaseId == 16185))
		{
			string value = ((gameObject.BaseId == 16183) ? "red" : ((gameObject.BaseId == 16184) ? "blue" : "yellow"));
			Vector2 value2 = gameObject.Position.ToVector2();
			Plugin.DebugLog($"cube color:{value}, position:{value2}");
			int num = ((int)value2.X - 88) / 8;
			int num2 = ((int)value2.Y - 84) / 8;
			cube[num, num2] = (int)(gameObject.BaseId - 16182);
			cubeGameObject[num, num2] = gameObject;
			cubeCount++;
			if (cubeCount == 12)
			{
				DrawLines(swap: false);
			}
			if (cubeCount == 24)
			{
				DrawLines(swap: true);
			}
		}
	}

	public override void Reset()
	{
		cubeCount = 0;
		base.Reset();
	}

	public ClassicalConcepts()
	{
		bias = new List<(int, int)>
		{
			(1, 0),
			(-1, 0),
			(0, 1),
			(0, -1)
		};
	}
}

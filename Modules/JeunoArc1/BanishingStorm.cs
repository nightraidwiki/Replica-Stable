using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.JeunoArc1;

public class BanishingStorm : ISpecialAction
{
	private static readonly WPos[] Positions = new WPos[6]
	{
		new WPos(815f, 415f),
		new WPos(800f, 385f),
		new WPos(785f, 400f),
		new WPos(785f, 385f),
		new WPos(815f, 400f),
		new WPos(800f, 415f)
	};

	private static readonly WDir[] Directions = new WDir[12]
	{
		4f * (-0.003f).Degrees().ToDirection(),
		4f * 119.997f.Degrees().ToDirection(),
		4f * (-120.003f).Degrees().ToDirection(),
		4f * 180f.Degrees().ToDirection(),
		4f * (-60.005f).Degrees().ToDirection(),
		4f * 60f.Degrees().ToDirection(),
		4f * 89.999f.Degrees().ToDirection(),
		4f * (-150.001f).Degrees().ToDirection(),
		4f * (-30.001f).Degrees().ToDirection(),
		4f * (-90.004f).Degrees().ToDirection(),
		4f * 29.996f.Degrees().ToDirection(),
		4f * 149.996f.Degrees().ToDirection()
	};

	private static readonly Dictionary<byte, (int position, int[] directions, int[] numExplosions)> LineConfigs = new Dictionary<byte, (int, int[], int[])>
	{
		{
			10,
			(0, new int[3] { 0, 1, 2 }, new int[3] { 5, 5, 14 })
		},
		{
			52,
			(0, new int[3] { 0, 1, 2 }, new int[3] { 5, 5, 14 })
		},
		{
			13,
			(0, new int[3] { 3, 5, 4 }, new int[3] { 13, 5, 9 })
		},
		{
			5,
			(3, new int[3] { 0, 1, 2 }, new int[3] { 13, 9, 5 })
		},
		{
			2,
			(3, new int[3] { 3, 5, 4 }, new int[3] { 5, 14, 5 })
		},
		{
			50,
			(3, new int[3] { 3, 5, 4 }, new int[3] { 5, 14, 5 })
		},
		{
			11,
			(1, new int[3] { 3, 4, 5 }, new int[3] { 5, 10, 10 })
		},
		{
			53,
			(1, new int[3] { 3, 4, 5 }, new int[3] { 5, 10, 10 })
		},
		{
			8,
			(1, new int[3] { 0, 2, 1 }, new int[3] { 13, 9, 9 })
		},
		{
			9,
			(2, new int[3] { 9, 11, 10 }, new int[3] { 5, 10, 10 })
		},
		{
			12,
			(2, new int[3] { 6, 7, 8 }, new int[3] { 13, 9, 9 })
		},
		{
			3,
			(4, new int[3] { 6, 7, 8 }, new int[3] { 5, 10, 10 })
		},
		{
			6,
			(4, new int[3] { 9, 11, 10 }, new int[3] { 13, 9, 9 })
		},
		{
			7,
			(5, new int[3] { 0, 1, 2 }, new int[3] { 5, 10, 10 })
		},
		{
			51,
			(5, new int[3] { 0, 1, 2 }, new int[3] { 5, 10, 10 })
		},
		{
			4,
			(5, new int[3] { 3, 5, 4 }, new int[3] { 13, 9, 9 })
		}
	};

	public override string Name => "Banishing Storm";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnEnvControl(byte index, uint state)
	{
		if (!LineConfigs.TryGetValue(index, out (int, int[], int[]) value) || state != 131073)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				WPos wPos = Positions[value.Item1] + j * Directions[value.Item2[i]];
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(wPos.X, 0f, wPos.Z),
					drawOnObject = false,
					radiusX = 6f,
					radiusZ = 6f,
					destroyTime = ((i == 0) ? 9100 : 9800) + j * 700
				});
			}
		}
	}
}

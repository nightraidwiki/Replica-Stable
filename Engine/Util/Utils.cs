using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Replica.Engine.Util;

public static class Utils
{
	public struct GridSnapper
	{
		public Vector3 Center;

		public float Size;

		public int GridCount;

		public readonly Vector3 Snap(Vector3 pos)
		{
			return SnapToGrid(pos, Center, Size, GridCount);
		}
	}

	public static void RotateList<T>(List<T> list, int startIndex)
	{
		int count = list.Count;
		if (count > 1 && startIndex != 0 && startIndex % count != 0)
		{
			startIndex %= count;
			Reverse(list, 0, startIndex - 1);
			Reverse(list, startIndex, count - 1);
			Reverse(list, 0, count - 1);
		}
	}

	public static Vector3 SnapToGrid(Vector3 pos, Vector3 center, float size, int gridCount)
	{
		float cell = size / (float)gridCount;
		float half = size / 2f;
		return new Vector3(SnapAxis(pos.X, center.X, half, cell, gridCount), pos.Y, SnapAxis(pos.Z, center.Z, half, cell, gridCount));
	}

	private static void Reverse<T>(List<T> list, int start, int end)
	{
		Span<T> span = CollectionsMarshal.AsSpan(list);
		while (start < end)
		{
			ref T reference = ref span[start];
			ref T reference2 = ref span[end];
			T val = span[end];
			T val2 = span[start];
			reference = val;
			reference2 = val2;
			start++;
			end--;
		}
	}

	private static float SnapAxis(float value, float center, float half, float cell, int gridCount)
	{
		int value2 = (int)MathF.Floor((value - (center - half)) / cell);
		value2 = Math.Clamp(value2, 0, gridCount - 1);
		return center - half + (float)value2 * cell + cell / 2f;
	}
}

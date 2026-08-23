using System;
using Lumina.Excel.Sheets;
using Replica.Engine.Enum;
using Replica.Engine.Interop;

namespace Replica.Engine.Util;

public static class ShapeUtil
{
	public static Shape GetShape(byte actionType)
	{
		return actionType switch
		{
			2 => Shape.Circle, 
			3 => Shape.Cone, 
			4 => Shape.Rectangle, 
			5 => Shape.Circle, 
			7 => Shape.Circle, 
			8 => Shape.RectToTarget, 
			10 => Shape.Donut, 
			11 => Shape.Cross, 
			12 => Shape.Rectangle, 
			13 => Shape.Cone, 
			14 => Shape.Triangle, 
			_ => Shape.None, 
		};
	}

	public static string GetGameTriangleOmen(int Degree)
	{
		return Degree switch
		{
			30 => "x6d3_b2_triangle30_p1", 
			60 => "x6d3_b2_triangle60_p1", 
			90 => "x6d3_b2_triangle90_p1", 
			_ => string.Empty, 
		};
	}

	public static string GetGameFanOmen(int Degree)
	{
		return Degree switch
		{
			15 => "gl_fan015_0x", 
			20 => "gl_fan020_0f", 
			30 => "gl_fan030_1bf", 
			40 => "gl_fan045_1bf", 
			45 => "gl_fan045_1bf", 
			60 => "gl_fan060_1bf", 
			90 => "gl_fan090_1bf", 
			120 => "gl_fan120_1bf", 
			130 => "gl_fan130_0x", 
			135 => "gl_fan135_c0g", 
			150 => "gl_fan150_1bf", 
			180 => "gl_fan180_1bf", 
			210 => "gl_fan210_1bf", 
			240 => "x6d3_b1_fan240_p1", 
			270 => "gl_fan270_0100af", 
			_ => "customFan", 
		};
	}

	public static int DetermineConeAngle(Lumina.Excel.Sheets.Action data)
	{
		Omen value = data.Omen.Value;
		if (value.RowId == 0)
		{
			Svc.Log.Warning($"No omen data for {data.RowId} '{data.Name}'...");
			return 90;
		}
		string text = value.Path.ToString();
		int num = text.IndexOf("fan", StringComparison.Ordinal);
		if (num < 0 || num + 6 > text.Length)
		{
			Svc.Log.Warning($"Can't determine angle from omen ({text}/{value.PathAlly}) for {data.RowId} '{data.Name}'...");
			return 90;
		}
		if (!int.TryParse(text.AsSpan(num + 3, 3), out var result))
		{
			Svc.Log.Warning($"Can't determine angle from omen ({text}/{value.PathAlly}) for {data.RowId} '{data.Name}'...");
			return 90;
		}
		Plugin.DebugLog($"{data.Name}({data.RowId}) Omen:({text}/{value.PathAlly}) Degrees:{result}° {result.Degrees()}");
		return result;
	}

	public static float RadiansToDegrees(this float radians)
	{
		return 360f - ((float)(180.0 / Math.PI * (double)radians) + 180f);
	}

	public static float IntToFloatAngle(this ushort rot)
	{
		return (float)(int)rot / 65535f * ((float)Math.PI * 2f) - (float)Math.PI;
	}

	public static int DetermineTriangleAngle(Lumina.Excel.Sheets.Action data)
	{
		Omen value = data.Omen.Value;
		if (value.RowId == 0)
		{
			Svc.Log.Warning($"No omen data for {data.RowId} '{data.Name}'...");
			return 90;
		}
		string text = value.Path.ToString();
		int num = text.IndexOf("triangle", StringComparison.Ordinal);
		if (num < 0 || num + 9 > text.Length)
		{
			Svc.Log.Warning($"Can't determine angle from omen ({text}/{value.PathAlly}) for {data.RowId} '{data.Name}'...");
			return 90;
		}
		if (!int.TryParse(text.AsSpan(num + 8, 2), out var result) && !int.TryParse(text.AsSpan(num + 8, 3), out result))
		{
			Svc.Log.Warning($"Can't determine angle from omen ({text}/{value.PathAlly}) for {data.RowId} '{data.Name}'...");
			return 90;
		}
		Plugin.DebugLog($"{data.Name}({data.RowId}) Omen:({text}/{value.PathAlly}) Degrees:{result}° {result.Degrees()}");
		return result;
	}
}

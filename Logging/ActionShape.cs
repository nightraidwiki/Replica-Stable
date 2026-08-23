using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Lumina.Excel.Sheets;
using Replica.QuickDraws;

namespace Replica.Logging;

public static class ActionShape
{
	public readonly record struct Info(string Label, string Call);

	public readonly record struct Geom(QuickShape Shape, float Radius, float HalfWidth, float Length, int FanAngle);

	public static Geom? Resolve(uint actionId)
	{
		if (actionId == 0)
		{
			return null;
		}
		Lumina.Excel.Sheets.Action? rowOrDefault = Plugin.Actions.GetRowOrDefault(actionId);
		if (!rowOrDefault.HasValue)
		{
			return null;
		}
		float num = (int)rowOrDefault.Value.EffectRange;
		float num2 = (int)rowOrDefault.Value.XAxisModifier;
		return rowOrDefault.Value.CastType switch
		{
			2 => new Geom(QuickShape.Circle, num, 0f, 0f, 0), 
			3 => new Geom(QuickShape.Fan, num, 0f, 0f, ConeAngle(rowOrDefault.Value)), 
			4 => new Geom(QuickShape.Rectangle, 0f, MathF.Max(0.5f, num2 * 0.5f), num, 0), 
			5 => new Geom(QuickShape.Donut, num, 0f, 0f, 0), 
			_ => null, 
		};
	}

	public static Info? Describe(uint actionId)
	{
		Geom? geom = Resolve(actionId);
		if (!geom.HasValue)
		{
			return null;
		}
		Geom value = geom.Value;
		return value.Shape switch
		{
			QuickShape.Circle => new Info("● " + Fmt(value.Radius), ".drawCircle(" + Fmt(value.Radius) + ")"), 
			QuickShape.Fan => new Info($"◔ {Fmt(value.Radius)} {value.FanAngle}°", $".drawCone({Fmt(value.Radius)}, {value.FanAngle})"), 
			QuickShape.Rectangle => new Info("▭ " + Fmt(value.Length) + "×" + Fmt(value.HalfWidth * 2f), $".drawLine({Fmt(value.Length)}, {Fmt(value.HalfWidth)})"), 
			QuickShape.Donut => new Info("◎ " + Fmt(value.Radius), ".drawDonut(?, " + Fmt(value.Radius) + ")"), 
			_ => null, 
		};
	}

	private static int ConeAngle(Lumina.Excel.Sheets.Action row)
	{
		float num = 90f;
		Omen? valueNullable = row.Omen.ValueNullable;
		if (valueNullable.HasValue)
		{
			Match match = Regex.Match(valueNullable.Value.Path.ExtractText(), "fan(\\d+)", RegexOptions.IgnoreCase);
			if (match.Success && float.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				num = result;
			}
		}
		return (int)num;
	}

	private static string Fmt(float v)
	{
		return v.ToString((v % 1f == 0f) ? "0" : "0.##", CultureInfo.InvariantCulture);
	}
}

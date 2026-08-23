using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Replica.Scripting.Api;

public class Event
{
	private const string RegexPrefix = "regex:";

	public DateTime DateTime { get; } = DateTime.Now;

	public EventTypeEnum Type { get; set; }

	public string Info { get; set; } = "";

	private ConcurrentDictionary<string, string> Properties { get; init; } = new ConcurrentDictionary<string, string>();

	public Dictionary<string, string> PropertiesCopy => new Dictionary<string, string>(Properties);

	public string this[string name]
	{
		get
		{
			if (!Properties.TryGetValue(name, out string value))
			{
				return "";
			}
			return value;
		}
		set
		{
			Properties[name] = value ?? "";
		}
	}

	public ulong SourceId => Hex(this["SourceId"]);

	public ulong TargetId => Hex(this["TargetId"]);

	public Vector3 SourcePosition => Vec(this["SourcePosition"]);

	public Vector3 TargetPosition => Vec(this["TargetPosition"]);

	public Vector3 EffectPosition => Vec(this["EffectPosition"]);

	public float SourceRotation => Num(this["SourceRotation"]);

	public float TargetRotation => Num(this["TargetRotation"]);

	public uint Id => Dec(this["Id"]);

	public uint ActionId => Dec(this["ActionId"]);

	public uint StatusId => Dec(this["StatusID"]);

	public uint StatusStackCount => Dec(this["StatusStackCount"]);

	public uint StatusParam => Dec(this["StatusParam"]);

	public void AddProperties(string name, string value)
	{
		Properties.TryAdd(name, value ?? "");
	}

	public bool Has(string name)
	{
		return Properties.ContainsKey(name);
	}

	public bool Has(IEnumerable<string> names)
	{
		foreach (string name in names)
		{
			if (!Properties.ContainsKey(name))
			{
				return false;
			}
		}
		return true;
	}

	public bool TryGet(string name, out string value)
	{
		if (Properties.TryGetValue(name, out string value2))
		{
			value = value2;
			return true;
		}
		value = "";
		return false;
	}

	public bool Match(KeyValuePair<string, string> pair)
	{
		try
		{
			if (string.IsNullOrEmpty(pair.Key) || string.IsNullOrEmpty(pair.Value))
			{
				return true;
			}
			if (!Properties.TryGetValue(pair.Key, out string value))
			{
				return false;
			}
			if (pair.Value.StartsWith("regex:", StringComparison.Ordinal))
			{
				return Regex.IsMatch(value, pair.Value.Substring("regex:".Length));
			}
			return value == pair.Value;
		}
		catch
		{
			return false;
		}
	}

	public bool Match(IEnumerable<KeyValuePair<string, string>> pairs)
	{
		foreach (KeyValuePair<string, string> pair in pairs)
		{
			if (!Match(pair))
			{
				return false;
			}
		}
		return true;
	}

	public Event Clone()
	{
		return new Event
		{
			Type = Type,
			Info = Info,
			Properties = new ConcurrentDictionary<string, string>(Properties)
		};
	}

	public static string Format(Vector3 v)
	{
		IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(16, 3, invariantCulture);
		handler.AppendLiteral("{\"X\":");
		handler.AppendFormatted(v.X);
		handler.AppendLiteral(",\"Y\":");
		handler.AppendFormatted(v.Y);
		handler.AppendLiteral(",\"Z\":");
		handler.AppendFormatted(v.Z);
		handler.AppendLiteral("}");
		return string.Create(invariantCulture, ref handler);
	}

	private static ulong Hex(string s)
	{
		if (!ulong.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
		{
			return 0uL;
		}
		return result;
	}

	private static uint Dec(string s)
	{
		if (!uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return 0u;
		}
		return result;
	}

	private static float Num(string s)
	{
		if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return 0f;
		}
		return result;
	}

	private static Vector3 Vec(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return Vector3.Zero;
		}
		float x = 0f;
		float y = 0f;
		float z = 0f;
		string[] array = s.Trim(new char[2] { '{', '}' }).Split(',');
		foreach (string text in array)
		{
			int num = text.IndexOf(':');
			if (num < 0)
			{
				continue;
			}
			ReadOnlySpan<char> span = text.AsSpan(0, num).Trim().Trim('"');
			if (float.TryParse(text.AsSpan(num + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				if (span.Equals("X".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					x = result;
				}
				else if (span.Equals("Y".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					y = result;
				}
				else if (span.Equals("Z".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					z = result;
				}
			}
		}
		return new Vector3(x, y, z);
	}
}

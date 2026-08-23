using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Replica.Engine.Properties;

internal static class Resources
{
	private static readonly Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();

	internal static byte[] eyewarn => Load("eyewarn");

	internal static byte[] Share2_6m_5s_omen => Load("Share2_6m_5s_omen");

	internal static byte[] ShareLazer5sGround => Load("ShareLazer5sGround");

	internal static byte[] tank_lockon_3m_5s_noc => Load("tank_lockon_3m_5s_noc");

	internal static byte[] tank_lockon_5m_5s_noc => Load("tank_lockon_5m_5s_noc");

	internal static byte[] tank_lockon_8s_noc => Load("tank_lockon_8s_noc");

	internal static byte[] TankFan90 => Load("TankFan90");

	internal static byte[] tmp_circle => Load("tmp_circle");

	internal static byte[] tmp_donut => Load("tmp_donut");

	internal static byte[] tmp_fan => Load("tmp_fan");

	internal static byte[] tmp_org_donut => Load("tmp_org_donut");

	internal static byte[] tmp_org_fan => Load("tmp_org_fan");

	internal static byte[] tmp_rect => Load("tmp_rect");

	internal static byte[] tmp_rect2 => Load("tmp_rect2");

	private static byte[] Load(string name)
	{
		if (Cache.TryGetValue(name, out byte[] value))
		{
			return value;
		}
		Assembly assembly = typeof(Resources).Assembly;
		string value2 = "Resources." + name + ".bin";
		string text = null;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text2 in manifestResourceNames)
		{
			if (text2.EndsWith(value2, StringComparison.Ordinal))
			{
				text = text2;
				break;
			}
		}
		if (text == null)
		{
			throw new InvalidOperationException("Missing embedded resource: " + name);
		}
		using Stream stream = assembly.GetManifestResourceStream(text) ?? throw new InvalidOperationException("Missing embedded resource stream: " + name);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		byte[] array = memoryStream.ToArray();
		Cache[name] = array;
		return array;
	}

	internal static byte[]? TryGet(string name)
	{
		try
		{
			return Load(name);
		}
		catch
		{
			return null;
		}
	}
}

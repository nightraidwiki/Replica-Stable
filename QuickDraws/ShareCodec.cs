using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Replica.QuickDraws;

public static class ShareCodec
{
	public const string ModulePrefix = "YAPDRAWPACK1:";

	public const string DrawPrefix = "YAPDRAW1:";

	public const string StratPrefix = "YAPSTRAT1:";

	private static readonly JsonSerializerOptions Opts = new JsonSerializerOptions
	{
		IncludeFields = true,
		WriteIndented = false
	};

	public static string Encode<T>(string prefix, T value)
	{
		string s = JsonSerializer.Serialize(value, Opts);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return prefix + Convert.ToBase64String(memoryStream.ToArray());
	}

	public static bool TryDecode<T>(string prefix, string code, out T? value)
	{
		value = default(T);
		if (string.IsNullOrWhiteSpace(code))
		{
			return false;
		}
		code = code.Trim();
		if (code.StartsWith(prefix, StringComparison.Ordinal))
		{
			code = code.Substring(prefix.Length);
		}
		try
		{
			using MemoryStream stream = new MemoryStream(Convert.FromBase64String(code));
			using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			gZipStream.CopyTo(memoryStream);
			string json = Encoding.UTF8.GetString(memoryStream.ToArray());
			value = JsonSerializer.Deserialize<T>(json, Opts);
			return value != null;
		}
		catch
		{
			return false;
		}
	}
}
